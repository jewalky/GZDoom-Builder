//-----------------------------------------------------------------------------
//
// Copyright(C) 1993-1996 Id Software, Inc.
// Copyright(C) 2005 Simon Howard
// Copyright(C) 2014 Andrew Apted
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
//	Do all the WAD I/O, get map description,
//	set up initial state and misc. LUTs.
//
//-----------------------------------------------------------------------------

#include "vpo_local.h"

namespace vpo
{


// the error message returned from P_SetupLevel()
static char  level_error_msg[1024];

// this exception is thrown during P_SetupLevel() when a map with bad data
// is detected (e.g. vertex number out of range).  The above message buffer
// should contain the error message.  [added by andrewj]
class invalid_data_exception { };


static bool level_is_hexen;


//
// MAP related Lookup tables.
// Store VERTEXES, LINEDEFS, SIDEDEFS, etc.
//
vertex_t*	vertexes;
int		numvertexes;

seg_t*		segs;
int		numsegs;

sector_t*	sectors;
int		numsectors;

subsector_t*	subsectors;
int		numsubsectors;

node_t*		nodes;
int		numnodes;

line_t*		lines;
int		numlines;

side_t*		sides;
int		numsides;


fixed_t  Map_bbox[4];


static int R_TextureNumForName (const char * name)
{
	// "NoTexture" marker.
	if (name[0] == '-')         
		return 0;

	return 1;  // dummy value
}

static int R_FlatNumForName (const char * name)
{
	// SKY ?
	if (name[0] == 'F' && name[1] == '_' && name[2] == 'S' && name[3] == 'K')
		return skyflatnum;

	return 1;  // dummy value
}


static void LevelError(const char *msg, ...)
{
	va_list argptr;

	va_start(argptr, msg);
	memset(level_error_msg, 0, sizeof(level_error_msg));
	vsnprintf(level_error_msg, sizeof(level_error_msg) - 1, msg, argptr);
	va_end(argptr);

	throw invalid_data_exception();
}


//
// P_LoadVertexes
//
void P_LoadVertexes (int lump)
{
	byte*		data;
	int			i;
	mapvertex_t*	ml;
	vertex_t*		li;

	// Determine number of lumps:
	//  total lump length / vertex record length.
	numvertexes = W_LumpLength (lump) / sizeof(mapvertex_t);

	// Allocate zone memory for buffer.
	vertexes = new vertex_t[numvertexes];

	// Load data into cache.
	data = W_LoadLump (lump);

	ml = (mapvertex_t *)data;
	li = vertexes;

	// Copy and convert vertex coordinates,
	// internal representation as fixed.
	for (i=0 ; i < numvertexes ; i++, li++, ml++)
	{
		li->x = SHORT(ml->x)<<FRACBITS;
		li->y = SHORT(ml->y)<<FRACBITS;
	}

	// Free buffer memory.
	W_FreeLump(data);
}


//
// GetSectorAtNullAddress
//
sector_t * GetSectorAtNullAddress(void)
{
	return sectors + 0;
}


//
// P_LoadSegs
//
void P_LoadSegs (int lump)
{
	byte*		data;
	int		i;
	mapseg_t*	ml;
	seg_t*		li;
	line_t*		ldef;
	int		side;
	int             sidenum;

	numsegs = W_LumpLength (lump) / sizeof(mapseg_t);
	segs = new seg_t[numsegs];

	memset (segs, 0, numsegs*sizeof(seg_t));

	data = W_LoadLump (lump);

	ml = (mapseg_t *)data;
	li = segs;

	for (i=0 ; i < numsegs ; i++, li++, ml++)
	{
		int v1_idx = SHORT(ml->v1);
		int v2_idx = SHORT(ml->v2);

		if (v1_idx < 0 || v1_idx >= numvertexes ||
		    v2_idx < 0 || v2_idx >= numvertexes)
		{
			LevelError("Bad map data : vertex out of range (seg #%d)", i);
		}

		li->v1 = &vertexes[v1_idx];
		li->v2 = &vertexes[v2_idx];

		int line_idx = SHORT(ml->linedef);

		if (line_idx < 0 || line_idx >= numlines)
		{
			LevelError("Bad map data : linedef out of range (seg #%d)", i);
		}

		li->angle = (SHORT(ml->angle))<<16;
		li->offset = (SHORT(ml->offset))<<16;

		ldef = &lines[line_idx];
		li->linedef = ldef;

		side = SHORT(ml->side);
		li->sidedef = &sides[ldef->sidenum[side]];
		li->frontsector = sides[ldef->sidenum[side]].sector;

		if (ldef-> flags & ML_TWOSIDED)
		{
			sidenum = ldef->sidenum[side ^ 1];

			// If the sidenum is out of range, this may be a "glass hack"
			// impassible window.  Point at side #0 (this may not be
			// the correct Vanilla behavior; however, it seems to work for
			// OTTAWAU.WAD, which is the one place I've seen this trick
			// used).

			if (sidenum < 0 || sidenum >= numsides)
			{
				li->backsector = GetSectorAtNullAddress();
			}
			else
			{
				li->backsector = sides[sidenum].sector;
			}
		}
		else
		{
			li->backsector = NULL;
		}
	}

	W_FreeLump(data);
}


//
// P_LoadSubsectors
//
void P_LoadSubsectors (int lump)
{
	byte*		data;
	int			i;
	mapsubsector_t*	ms;
	subsector_t*	ss;

	numsubsectors = W_LumpLength (lump) / sizeof(mapsubsector_t);
	subsectors = new subsector_t[numsubsectors];

	data = W_LoadLump (lump);

	ms = (mapsubsector_t *)data;
	memset (subsectors,0, numsubsectors*sizeof(subsector_t));
	ss = subsectors;

	for (i=0 ; i < numsubsectors ; i++, ss++, ms++)
	{
		ss->numlines = SHORT(ms->numsegs);
		ss->firstline = SHORT(ms->firstseg);
	}

	W_FreeLump(data);
}


// andrewj: added this
static void ValidateSubsectors ()
{
	int i;
	subsector_t * ss = subsectors;

	for (i=0 ; i < numsubsectors ; i++, ss++)
	{
		if (ss->firstline < 0 || ss->numlines < 0 ||
		    ss->firstline + ss->numlines > numsegs)
		{
			LevelError("Bad map data : invalid seg range in subsector #%d\n", i);
		}
	}
}


//
// P_LoadSectors
//
void P_LoadSectors (int lump)
{
	byte*		data;
	int			i;
	mapsector_t*	ms;
	sector_t*		ss;

	numsectors = W_LumpLength (lump) / sizeof(mapsector_t);
	sectors = new sector_t[numsectors];

	memset (sectors, 0, numsectors*sizeof(sector_t));
	data = W_LoadLump (lump);

	ms = (mapsector_t *)data;
	ss = sectors;

	for (i=0 ; i < numsectors ; i++, ss++, ms++)
	{
		ss->floorheight = SHORT(ms->floorheight)<<FRACBITS;
		ss->ceilingheight = SHORT(ms->ceilingheight)<<FRACBITS;
		ss->floorpic = R_FlatNumForName(ms->floorpic);
		ss->ceilingpic = R_FlatNumForName(ms->ceilingpic);
		ss->lightlevel = SHORT(ms->lightlevel);
		ss->special = SHORT(ms->special);
		ss->tag = SHORT(ms->tag);
		///	ss->thinglist = NULL;
	}

	W_FreeLump(data);
}


static bool isChildValid(unsigned short child)
{
	if (child & NF_SUBSECTOR)
		return ((child & ~NF_SUBSECTOR) < numsubsectors);
	else
		return (child < numnodes);
}


//
// P_LoadNodes
//
void P_LoadNodes (int lump)
{
	byte*	data;
	int		i;
	int		j;
	int		k;
	mapnode_t*	mn;
	node_t*	no;

	numnodes = W_LumpLength (lump) / sizeof(mapnode_t);
	nodes = new node_t[numnodes];

	data = W_LoadLump (lump);

	mn = (mapnode_t *)data;
	no = nodes;

	for (i=0 ; i < numnodes ; i++, no++, mn++)
	{
		no->x = SHORT(mn->x)<<FRACBITS;
		no->y = SHORT(mn->y)<<FRACBITS;
		no->dx = SHORT(mn->dx)<<FRACBITS;
		no->dy = SHORT(mn->dy)<<FRACBITS;

		for (j=0 ; j < 2 ; j++)
		{
			unsigned short child = SHORT(mn->children[j]);

			if (! isChildValid(child))
				LevelError("Bad map data : invalid child in node #%d", i);

			no->children[j] = child;

			for (k=0 ; k < 4 ; k++)
				no->bbox[j][k] = SHORT(mn->bbox[j][k])<<FRACBITS;
		}
	}

	W_FreeLump(data);
}


/* andrewj : removed P_LoadThings(), not needed for Visplane Explorer */


static void LineDef_CommonSetup(line_t *ld)
{
	vertex_t *v1 = ld->v1;
	vertex_t *v2 = ld->v2;

	ld->dx = v2->x - v1->x;
	ld->dy = v2->y - v1->y;

	if (!ld->dx)
		ld->slopetype = ST_VERTICAL;
	else if (!ld->dy)
		ld->slopetype = ST_HORIZONTAL;
	else
	{
		if (FixedDiv (ld->dy , ld->dx) > 0)
			ld->slopetype = ST_POSITIVE;
		else
			ld->slopetype = ST_NEGATIVE;
	}

	if (v1->x < v2->x)
	{
		ld->bbox[BOXLEFT] = v1->x;
		ld->bbox[BOXRIGHT] = v2->x;
	}
	else
	{
		ld->bbox[BOXLEFT] = v2->x;
		ld->bbox[BOXRIGHT] = v1->x;
	}

	if (v1->y < v2->y)
	{
		ld->bbox[BOXBOTTOM] = v1->y;
		ld->bbox[BOXTOP] = v2->y;
	}
	else
	{
		ld->bbox[BOXBOTTOM] = v2->y;
		ld->bbox[BOXTOP] = v1->y;
	}

	// andrewj: be tolerant of bad sidedef numbers
	if (ld->sidenum[0] < 0 || ld->sidenum[0] >= numsides)
	{
		if (numsides == 0)
			LevelError("Bad map data : no sidedefs!");

		ld->sidenum[0] = 0;
	}

	if (ld->sidenum[1] < -1 || ld->sidenum[1] >= numsides)
		ld->sidenum[1] = -1;


	if (ld->sidenum[0] != -1)
		ld->frontsector = sides[ld->sidenum[0]].sector;
	else
		ld->frontsector = NULL;

	if (ld->sidenum[1] != -1)
		ld->backsector = sides[ld->sidenum[1]].sector;
	else
		ld->backsector = NULL;
}


//
// P_LoadLineDefs
// Also counts secret lines for intermissions.
//
void P_LoadLineDefs (int lump)
{
	byte*		data;
	int		i;
	maplinedef_t*	mld;
	line_t*		ld;

	numlines = W_LumpLength (lump) / sizeof(maplinedef_t);
	lines = new line_t[numlines];

	memset (lines, 0, numlines*sizeof(line_t));
	data = W_LoadLump (lump);

	mld = (maplinedef_t *)data;
	ld = lines;

	for (i=0 ; i < numlines ; i++, mld++, ld++)
	{
		int v1_idx = SHORT(mld->v1);
		int v2_idx = SHORT(mld->v2);

		if (v1_idx < 0 || v1_idx >= numvertexes ||
		    v2_idx < 0 || v2_idx >= numvertexes)
		{
			LevelError("Bad map data : vertex out of range (line #%d)", i);
		}

		ld->v1 = &vertexes[v1_idx];
		ld->v2 = &vertexes[v2_idx];
		
		ld->flags = SHORT(mld->flags);
		ld->special = SHORT(mld->special);
		ld->tag = SHORT(mld->tag);
		ld->sidenum[0] = SHORT(mld->sidenum[0]);
		ld->sidenum[1] = SHORT(mld->sidenum[1]);

		LineDef_CommonSetup(ld);
	}

	W_FreeLump(data);
}


// andrewj: added this for Hexen support
void P_LoadLineDefs_Hexen (int lump)
{
	byte*			data;
	int			i, k;
	maplinedef_hexen_t*	mld;
	line_t*			ld;

	numlines = W_LumpLength (lump) / sizeof(maplinedef_hexen_t);
	lines = new line_t[numlines];

	memset (lines, 0, numlines*sizeof(line_t));
	data = W_LoadLump (lump);

	mld = (maplinedef_hexen_t *)data;
	ld = lines;

	for (i=0 ; i < numlines ; i++, mld++, ld++)
	{
		int v1_idx = SHORT(mld->v1);
		int v2_idx = SHORT(mld->v2);

		if (v1_idx < 0 || v1_idx >= numvertexes ||
		    v2_idx < 0 || v2_idx >= numvertexes)
		{
			LevelError("Bad map data : vertex out of range (line #%d)", i);
		}

		ld->v1 = &vertexes[v1_idx];
		ld->v2 = &vertexes[v2_idx];

		ld->flags = SHORT(mld->flags);
		ld->special = mld->special;
		ld->tag = 0;
		ld->sidenum[0] = SHORT(mld->sidenum[0]);
		ld->sidenum[1] = SHORT(mld->sidenum[1]);

		for (k = 0 ; k < 5 ; k++)
			ld->args[k] = mld->args[k];

		LineDef_CommonSetup(ld);
	}

	W_FreeLump(data);
}


//
// P_LoadSideDefs
//
void P_LoadSideDefs (int lump)
{
	byte*		data;
	int			i;
	mapsidedef_t*	msd;
	side_t*		sd;

	numsides = W_LumpLength (lump) / sizeof(mapsidedef_t);
	sides = new side_t[numsides];

	memset (sides, 0, numsides*sizeof(side_t));
	data = W_LoadLump (lump);

	msd = (mapsidedef_t *)data;
	sd = sides;

	for (i=0 ; i < numsides ; i++, msd++, sd++)
	{
		int sec_idx = SHORT(msd->sector);

		// andrewj : silently fix a bad sector number
		if (sec_idx < 0 || sec_idx >= numsectors)
		{
			if (numsectors == 0)
				LevelError("Bad map data : no sectors!");

			sec_idx = 0;
		}

		sd->textureoffset = SHORT(msd->textureoffset)<<FRACBITS;
		sd->rowoffset = SHORT(msd->rowoffset)<<FRACBITS;
		sd->toptexture = R_TextureNumForName(msd->toptexture);
		sd->bottomtexture = R_TextureNumForName(msd->bottomtexture);
		sd->midtexture = R_TextureNumForName(msd->midtexture);
		sd->sector = &sectors[sec_idx];
	}

	W_FreeLump(data);
}



//
// P_GroupLines
// Builds sector line lists and subsector sector numbers.
// Finds block bounding boxes for sectors.
//
void P_GroupLines (void)
{
	line_t**		linebuffer;
	int			i;
	int			j;
	line_t*		li;
	sector_t*		sector;
	subsector_t*	ss;
	seg_t*		seg;
	fixed_t		bbox[4];
	int  totallines;

	// look up sector number for each subsector
	ss = subsectors;
	for (i=0 ; i < numsubsectors ; i++, ss++)
	{
		seg = &segs[ss->firstline];
		ss->sector = seg->sidedef->sector;
	}

	// count number of lines in each sector
	li = lines;
	totallines = 0;
	for (i=0 ; i < numlines ; i++, li++)
	{
		totallines++;
		li->frontsector->linecount++;

		if (li->backsector && li->backsector != li->frontsector)
		{
			li->backsector->linecount++;
			totallines++;
		}
	}

	// build line tables for each sector	
	linebuffer = new line_t* [totallines];

	for (i=0; i < numsectors; ++i)
	{
		// Assign the line buffer for this sector

		sectors[i].lines = linebuffer;
		linebuffer += sectors[i].linecount;

		// Reset linecount to zero so in the next stage we can count
		// lines into the list.

		sectors[i].linecount = 0;
	}

	// Assign lines to sectors

	for (i=0; i < numlines; ++i)
	{ 
		li = &lines[i];

		if (li->frontsector != NULL)
		{
			sector = li->frontsector;

			sector->lines[sector->linecount] = li;
			++sector->linecount;
		}

		if (li->backsector != NULL && li->frontsector != li->backsector)
		{
			sector = li->backsector;

			sector->lines[sector->linecount] = li;
			++sector->linecount;
		}
	}

	// Generate bounding boxes for sectors

	sector = sectors;
	for (i=0 ; i < numsectors ; i++, sector++)
	{
		M_ClearBox (bbox);

		for (j=0 ; j < sector->linecount; j++)
		{
			li = sector->lines[j];

			M_AddToBox (bbox, li->v1->x, li->v1->y);
			M_AddToBox (bbox, li->v2->x, li->v2->y);
		}
	}

	// andrewj : generate bounding box for level

	M_ClearBox (Map_bbox);

	for (i=0; i < numlines; ++i)
	{ 
		li = &lines[i];

		M_AddToBox (Map_bbox, li->v1->x, li->v1->y);
		M_AddToBox (Map_bbox, li->v2->x, li->v2->y);
	}
}


//
// andrewj: added this
//
// Find sectors which seem to be doors.
// Main criterion is that sector is closed, and either has a tag
// or one of the linedefs has a manual door type.
//
static int HasManualDoor(const sector_t *sec)
{
	int k;

	for (k = 0 ; k < sec->linecount ; k++)
	{
		const line_t *L = sec->lines[k];

		if (level_is_hexen)
		{
			switch (L->special)
			{
				case 10: case 11: case 12: case 13:
				case 202: /* zdoom's Generic_Door */
				{
					if (L->args[0] == 0)
						return +1;
				}
				default: break;
			}
		}
		else
		{
			switch (L->special)
			{
				case 1: case 26: case 27: case 28:
				case 31: case 32: case 33: case 34:
				case 117: case 118:
					return +1;

				default: break;
			}
		}
	}

	return 0;
}

static void CalcDoorAltHeight(sector_t *sec)
{
	fixed_t door_h = sec->floorheight;  // == sec->ceilingheight

	// compute lowest ceiling and highest floor of neighbor sectors
	fixed_t low_ceil   =  32767 << FRACBITS;
	fixed_t high_floor = -32767 << FRACBITS;

	int k, pass;

	for (k = 0 ; k < sec->linecount ; k++)
	{
		const line_t *L = sec->lines[k];

		for (pass = 0 ; pass < 2 ; pass++)
		{
			const sector_t *nb = pass ? L->backsector : L->frontsector;

			if (nb && nb != sec)
			{
				if (low_ceil > nb->ceilingheight)
					low_ceil = nb->ceilingheight;

				if (high_floor < nb->floorheight)
					high_floor = nb->floorheight;
			}
		}
	}

	// see if this is actually a lowering floor
	// (like used on MAP12 of DOOM 2)

	fixed_t mid_h = (low_ceil >> 1) + (high_floor >> 1);

	if (door_h > mid_h)
	{
		sec->is_door = -1;
		sec->alt_height = high_floor;
	}
	else
	{
		// is_door already set to +1
		sec->alt_height = low_ceil - (4 * FRACUNIT);
	}
}

void P_DetectDoorSectors(void)
{
	int i;

	for (i = 0 ; i < numsectors ; i++)
	{
		sector_t *sec = &sectors[i];

		if (sec->floorheight != sec->ceilingheight)
			continue;

		if (sec->tag || HasManualDoor(sec))
		{
			sec->is_door = +1;

			CalcDoorAltHeight(sec);
		}
	}
}


//
// P_SetupLevel
//
// Returns an error message if something went wrong
// or NULL on success.
//
const char * P_SetupLevel ( const char *lumpname, bool *is_hexen )
{
	int base = W_CheckNumForName(lumpname);

	if (base < 0 || ! lumpinfo[base].is_map_header)
	{
		sprintf(level_error_msg, "No such map in wad: %s", lumpname);
		return level_error_msg;
	}

	level_is_hexen = lumpinfo[base].is_hexen;

	if (is_hexen)
		*is_hexen = level_is_hexen;

	// check that we have some nodes
	if (lumpinfo[base + ML_SEGS].size == 0)
	{
		sprintf(level_error_msg, "Missing nodes for: %s", lumpname);
		return level_error_msg;
	}

	W_BeginRead();

	try
	{
		// note: most of this ordering is important	
		///    P_LoadBlockMap (base + ML_BLOCKMAP);
		P_LoadVertexes (base + ML_VERTEXES);
		P_LoadSectors (base + ML_SECTORS);
		P_LoadSideDefs (base + ML_SIDEDEFS);

		if (level_is_hexen)
			P_LoadLineDefs_Hexen (base + ML_LINEDEFS);
		else
			P_LoadLineDefs (base + ML_LINEDEFS);

		P_LoadSubsectors (base + ML_SSECTORS);
		P_LoadNodes (base + ML_NODES);
		P_LoadSegs (base + ML_SEGS);

		ValidateSubsectors();
	}
	catch (invalid_data_exception)
	{
		W_EndRead();
		P_FreeLevelData();

		return level_error_msg;
	}

	W_EndRead();

	P_GroupLines ();

	// andrewj: added this
	P_DetectDoorSectors();

	return NULL;
}


void P_FreeLevelData (void)
{
	if (vertexes)
	{
		delete[] vertexes;
		vertexes = NULL;
		numvertexes = 0;
	}

	if (sectors)
	{
		delete[] sectors;
		sectors = NULL;
		numsectors = 0;
	}

	if (sides)
	{
		delete[] sides;
		sides = NULL;
		numsides = 0;
	}

	if (lines)
	{
		delete[] lines;
		lines = NULL;
		numlines = 0;
	}

	if (segs)
	{
		delete[] segs;
		segs = NULL;
		numsegs = 0;
	}

	if (subsectors)
	{
		delete[] subsectors;
		subsectors = NULL;
		numsubsectors = 0;
	}

	if (nodes)
	{
		delete[] nodes;
		nodes = NULL;
		numnodes = 0;
	}
}


} // namespace vpo

//--- editor settings ---
// vi:ts=4:sw=4:noexpandtab
// Emacs style mode select   -*- C++ -*- 
