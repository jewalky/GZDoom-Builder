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
//	WAD I/O functions.
//
//-----------------------------------------------------------------------------

#ifndef __W_WAD__
#define __W_WAD__

#include <stdio.h>

#include "doomtype.h"
#include "w_file.h"


//
// TYPES
//

//
// WADFILE I/O related stuff.
//

typedef struct lumpinfo_s lumpinfo_t;

struct lumpinfo_s
{
	char	name[8];

	int		position;
	int		size;

	bool  is_map_header;  // e.g. MAP01 or E1M1 
	bool  is_hexen;
};


extern lumpinfo_t *lumpinfo;
extern int numlumps;


// andrewj: only a single file can be added now
bool W_AddFile (const char *filename);

void W_RemoveFile(void);


int	W_CheckNumForName (const char *name);

int	W_LumpLength (int lumpnum);


// andrewj: all lump loading must occur between these calls
void W_BeginRead(void);
void W_EndRead();

byte * W_LoadLump(int lumpnum);
void   W_FreeLump(byte * data);


#endif  /* __W_WAD__ */

//--- editor settings ---
// vi:ts=4:sw=4:noexpandtab
// Emacs style mode select   -*- C++ -*- 
