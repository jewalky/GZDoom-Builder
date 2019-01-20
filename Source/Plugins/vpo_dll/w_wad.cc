//-----------------------------------------------------------------------------
//
// Copyright(C) 1993-1996 Id Software, Inc.
// Copyright(C) 2005 Simon Howard
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA
// 02111-1307, USA.
//
// DESCRIPTION:
//	Handles WAD file header, directory, lump I/O.
//
//-----------------------------------------------------------------------------

#include "vpo_local.h"

namespace vpo
{


typedef struct
{
	// Should be "IWAD" or "PWAD".
	char		identification[4];
	int			numlumps;
	int			infotableofs;

} PACKEDATTR wadinfo_t;


typedef struct
{
	int			filepos;
	int			size;
	char		name[8];

} PACKEDATTR filelump_t;

//
// GLOBALS
//

// Location of each lump on disk.

lumpinfo_t *lumpinfo;

int numlumps = 0;

static char * wad_filename;

static wad_file_t * current_file;



//
// LUMP BASED ROUTINES.
//

// andrewj: added this to find level names
//          returns 0 if not a level, 1 for DOOM, 2 for HEXEN format
static int CheckMapHeader(filelump_t *lumps, int num_after)
{
	static const char *level_lumps[] =
	{
		"THINGS", "LINEDEFS", "SIDEDEFS", "VERTEXES",
		"SEGS", "SSECTORS", "NODES", "SECTORS",
		"REJECT", "BLOCKMAP"
	};

	if (num_after < 10)
		return 0;

	for (int i = 0 ; i < 10 ; i++)
	{
		const char *name = level_lumps[i];

		// level lumps are never valid map header names
		if (strncmp(lumps[0].name, name, 8) == 0)
			return 0;

		// require a one-to-one correspondence
		// (anything else would crash DOOM)
		if (strncmp(lumps[1 + i].name, name, 8) != 0)
			return 0;
	}

	// hexen format is distinguished by a following BEHAVIOR lump
	if (num_after >= 11 && strncmp(lumps[11].name, "BEHAVIOR", 8) == 0)
		return 2;

	return 1;
}


//
// W_AddFile
//
// All files are optional, but at least one file must be
//  found (PWAD, if all required lumps are present).
// Files with a .wad extension are wadlink files
//  with multiple lumps.
// Other files are single lumps with the base filename
//  for the lump name.

bool W_AddFile (const char *filename)
{
	wadinfo_t header;
	lumpinfo_t *lump_p;
	wad_file_t *wad_file;

	int i;
	int length;
	int startlump;

	filelump_t *fileinfo;
	filelump_t *filerover;

	// open the file and add to directory

	wad_file = W_OpenFile(filename);

	if (wad_file == NULL)
	{
		return false;
	}

	startlump = numlumps;

	W_Read(wad_file, 0, &header, sizeof(header));

	if (strncmp(header.identification,"IWAD",4) != 0)
	{
		// Homebrew levels?
		if (strncmp(header.identification,"PWAD",4) != 0)
		{
///			I_Error ("Wad file %s doesn't have IWAD "
///					"or PWAD id\n", filename);

			W_CloseFile(wad_file);

			return false;
		}
	}

	header.numlumps = LONG(header.numlumps);
	header.infotableofs = LONG(header.infotableofs);

	length = header.numlumps * sizeof(filelump_t);

	fileinfo = new filelump_t[header.numlumps];

	W_Read(wad_file, header.infotableofs, fileinfo, length);
	numlumps += header.numlumps;


	// Fill in lumpinfo
	lumpinfo = (lumpinfo_t *)realloc(lumpinfo, numlumps * sizeof(lumpinfo_t));

	if (! lumpinfo)
		I_Error ("Out of memory -- could not realloc lumpinfo");

	lump_p = &lumpinfo[startlump];

	filerover = fileinfo;

	for (i=startlump; i < numlumps; ++i)
	{
		int map_header;

		lump_p->position = LONG(filerover->filepos);
		lump_p->size = LONG(filerover->size);

		strncpy(lump_p->name, filerover->name, 8);

		map_header = CheckMapHeader(filerover, numlumps - i - 1);

		lump_p->is_map_header = (map_header >= 1);
		lump_p->is_hexen      = (map_header == 2);

		++lump_p;
		++filerover;
	}

	delete[] fileinfo;

	// close the file now, we re-open it later to load the map
	W_CloseFile(wad_file);

	wad_filename = strdup(filename);

	if (! wad_filename)
		I_Error ("Out of memory -- could not strdup filename");

	return true;
}


void W_RemoveFile(void)
{
	if (wad_filename)
	{
		free(wad_filename);

		wad_filename = NULL;

		free(lumpinfo);

		lumpinfo = NULL;
		numlumps = 0;
	}
}


//
// W_NumLumps
//
int W_NumLumps (void)
{
	return numlumps;
}


//
// W_CheckNumForName
// Returns -1 if name not found.
//
int W_CheckNumForName (const char* name)
{
	int i;

	// scan backwards so patch lump files take precedence

	for (i=numlumps-1; i >= 0; --i)
	{
		if (strncasecmp(lumpinfo[i].name, name, 8) == 0)
		{
			return i;
		}
	}

	// TFB. Not found.

	return -1;
}


//
// W_LumpLength
// Returns the buffer size needed to load the given lump.
//
int W_LumpLength (int lumpnum)
{
	if (lumpnum >= numlumps)
	{
		I_Error ("W_LumpLength: %i >= numlumps", lumpnum);
	}

	return lumpinfo[lumpnum].size;
}


//
// W_ReadLump
// Loads the lump into the given buffer,
//  which must be >= W_LumpLength().
//
static void W_ReadLump(int lump, void *dest)
{
	int c;
	lumpinfo_t *l;

	if (lump >= numlumps)
	{
		I_Error ("W_ReadLump: %i >= numlumps", lump);
	}

	l = lumpinfo+lump;

	c = W_Read(current_file, l->position, dest, l->size);

	if (c < l->size)
	{
		I_Error ("W_ReadLump: only read %i of %i on lump %i", c, l->size, lump);
	}
}


//
// W_LoadLump
//
// Load a lump into memory and return a pointer to a buffer containing
// the lump data.
//
byte * W_LoadLump(int lumpnum)
{
	byte *result;

	if (lumpnum < 0 || lumpnum >= numlumps)
	{
		I_Error ("W_LoadLump: %i >= numlumps", lumpnum);
	}

	if (! current_file)
		I_Error ("W_LoadLump: no current file (W_BeginRead not called)");

	// load it now

	result = new byte[W_LumpLength(lumpnum) + 1];

	W_ReadLump (lumpnum, result);

	return result;
}


void W_FreeLump(byte * data)
{
	delete[] data;
}


void W_BeginRead(void)
{
	// check API usage
	if (! wad_filename)
		I_Error("W_BeginRead called without any wad file!");

	if (current_file)
		I_Error("W_BeginRead called twice without W_EndRead.");

	current_file = W_OpenFile(wad_filename);

	// it should normally succeed, as it is unlikely the file suddenly
	// disappears between reading the directory and loading a map.
	if (! current_file)
		I_Error("Could not re-open the wad file -- deleted?");
}


void W_EndRead()
{
	if (! current_file)
		I_Error("W_EndRead called without a previous W_BeginRead.");

	W_CloseFile(current_file);

	current_file = NULL;
}


} // namespace vpo

//--- editor settings ---
// vi:ts=4:sw=4:noexpandtab
// Emacs style mode select   -*- C++ -*- 
