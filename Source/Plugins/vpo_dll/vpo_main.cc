//------------------------------------------------------------------------
//  Visplane Overflow Library
//------------------------------------------------------------------------
//
//  Copyright (C) 1993-1996 Id Software, Inc.
//  Copyright (C) 2005      Simon Howard
//  Copyright (C) 2012-2014 Andrew Apted
//
//  This program is free software; you can redistribute it and/or
//  modify it under the terms of the GNU General Public License
//  as published by the Free Software Foundation; either version 2
//  of the License, or (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//------------------------------------------------------------------------

#include "vpo_local.h"
#include "vpo_api.h"


static char error_buffer[1024];

// cache for the sector lookup
static int last_x, last_y;
static vpo::sector_t *last_sector;


static void ClearError(void)
{
	strcpy(error_buffer, "(No Error)");
}

static void SetError(const char *msg, ...)
{
	va_list argptr;

	va_start(argptr, msg);
	memset(error_buffer, 0, sizeof(error_buffer));
	vsnprintf(error_buffer, sizeof(error_buffer) - 1, msg, argptr);
	va_end(argptr);
}


const char *VPO_GetError(void)
{
	return error_buffer;
}


//------------------------------------------------------------------------

int VPO_LoadWAD(const char *wad_filename)
{
	ClearError();

	// free any previously loaded wad
	VPO_FreeWAD();

	vpo::R_Init();

	if (! vpo::W_AddFile(wad_filename))
	{
		SetError("Missing or invalid wad file: %s", wad_filename);
		return -1;
	}

	return 0;  // OK !
}


int VPO_OpenMap(const char *map_name, bool *is_hexen)
{
	// check a wad is loaded
	if (vpo::numlumps <= 0)
	{
		SetError("VPO_OpenMap called without any loaded wad");
		return -1;
	}

	ClearError();

	// close any previously loaded map
	VPO_CloseMap();

	const char *err_msg = vpo::P_SetupLevel(map_name, is_hexen);

	if (err_msg)
	{
		SetError("%s", err_msg);
		return -1;
	}

	return 0;  // OK !
}


void VPO_FreeWAD(void)
{
	VPO_CloseMap();

	vpo::W_RemoveFile();
}


void VPO_CloseMap(void)
{
	ClearError();

	last_x = -77777;
	last_y = -77777;
	last_sector = NULL;

	vpo::P_FreeLevelData();
}


const char * VPO_GetMapName(unsigned int index, bool *is_hexen)
{
	static char buffer[16];

	for (int lump_i = 0 ; lump_i < vpo::numlumps ; lump_i++)
	{
		if (! vpo::lumpinfo[lump_i].is_map_header)
			continue;

		if (index == 0)
		{
			// found it
			memcpy(buffer, vpo::lumpinfo[lump_i].name, 8);
			buffer[8] = 0;

			if (is_hexen)
				*is_hexen = vpo::lumpinfo[lump_i].is_hexen;

			return buffer;
		}
		
		index--;
	}

	// not found
	return NULL;
}


int VPO_GetLinedef(unsigned int index, int *x1, int *y1, int *x2, int *y2)
{
	if (index >= (unsigned int)vpo::numlines)
		return -1;

	const vpo::line_t *L = &vpo::lines[index];

	*x1 = L->v1->x >> FRACBITS;
	*y1 = L->v1->y >> FRACBITS;

	*x2 = L->v2->x >> FRACBITS;
	*y2 = L->v2->y >> FRACBITS;

	return L->backsector ? 2 : 1;
}


int VPO_GetSeg(unsigned int index, int *linedef, int *side,
               int *x1, int *y1, int *x2, int *y2)
{
	if (index >= (unsigned int)vpo::numsegs)
		return -1;
	
	const vpo::seg_t *seg = &vpo::segs[index];
	const vpo::line_t *L  = seg->linedef;

	*x1 = seg->v1->x >> FRACBITS;
	*y1 = seg->v1->y >> FRACBITS;

	*x2 = seg->v2->x >> FRACBITS;
	*y2 = seg->v2->y >> FRACBITS;

	*linedef = (L - vpo::lines);
	*side = 0;

	if (L->sidenum[1] >= 0 && seg->sidedef == &vpo::sides[L->sidenum[1]])
		*side = 1;

	return 0;
}


void VPO_GetBBox(int *x1, int *y1, int *x2, int *y2)
{
	*x1 = (vpo::Map_bbox[vpo::BOXLEFT]   >> FRACBITS);
	*y1 = (vpo::Map_bbox[vpo::BOXBOTTOM] >> FRACBITS);
	*x2 = (vpo::Map_bbox[vpo::BOXRIGHT]  >> FRACBITS);
	*y2 = (vpo::Map_bbox[vpo::BOXTOP]    >> FRACBITS);
}


void VPO_OpenDoorSectors(int dir)
{
	for (int i = 0 ; i < vpo::numsectors ; i++)
	{
		vpo::sector_t *sec = &vpo::sectors[i];

		if (sec->is_door == 0)
			continue;

		if (dir > 0)  // open them
		{
			if (sec->is_door > 0)
				sec->ceilingheight = sec->alt_height;
			else
				sec->floorheight = sec->alt_height;
		}
		else if (dir < 0)  // close them
		{
			if (sec->is_door > 0)
				sec->ceilingheight = sec->floorheight;
			else
				sec->floorheight = sec->ceilingheight;
		}
	}
}


//------------------------------------------------------------------------

int VPO_TestSpot(int x, int y, int dz, int angle,
                 int *num_visplanes, int *num_drawsegs,
                 int *num_openings,  int *num_solidsegs)
{
	// the actual spot we will use
	// (this prevents issues with X_SectorForPoint getting the wrong
	//  value when the casted ray hits a vertex)
	vpo::fixed_t rx = (x << FRACBITS) + (FRACUNIT / 2);
	vpo::fixed_t ry = (y << FRACBITS) + (FRACUNIT / 2);

	// check if spot is outside the map
	if (rx < vpo::Map_bbox[vpo::BOXLEFT]   ||
	    rx > vpo::Map_bbox[vpo::BOXRIGHT]  ||
	    ry < vpo::Map_bbox[vpo::BOXBOTTOM] ||
		ry > vpo::Map_bbox[vpo::BOXTOP])
	{
		return RESULT_IN_VOID;
	}

	// optimization: we cache the last sector lookup
	vpo::sector_t *sec;

	if (x == last_x && y == last_y)
		sec = last_sector;
	else
	{
		sec = vpo::X_SectorForPoint(rx, ry);

		last_x = x;
		last_y = y;
		last_sector = sec;
	}

	if (! sec)
		return RESULT_IN_VOID;
	
	vpo::fixed_t rz;
	
	if (dz < 0)
		rz = sec->ceilingheight + (dz << FRACBITS);
	else
		rz = sec->floorheight + (dz << FRACBITS);

	if (rz <= sec->floorheight || rz >= sec->ceilingheight)
		return RESULT_BAD_Z;

	// convert angle to the 32-bit BAM representation
	if (angle == 360)
		angle = 0;

	vpo::fixed_t ang2 = vpo::FixedDiv(angle << FRACBITS, 360 << FRACBITS);

	vpo::angle_t r_ang = (vpo::angle_t) (ang2 << 16);

	int result = RESULT_OK;

	// perform a no-draw render and see how many visplanes were needed
	try
	{
		vpo::R_RenderView(rx, ry, rz, r_ang);
	}
	catch (vpo::overflow_exception& e)
	{
		result = RESULT_OVERFLOW;
	}

	*num_visplanes = MAX(*num_visplanes, vpo::total_visplanes);
	*num_drawsegs  = MAX(*num_drawsegs,  vpo::total_drawsegs);
	*num_openings  = MAX(*num_openings,  vpo::total_openings);
	*num_solidsegs = MAX(*num_solidsegs, vpo::max_solidsegs);

	return result;
}


//------------------------------------------------------------------------

#if 0 // VPO_TEST_PROGRAM

// NOTE: this is out of date and will not compile

#define EYE_HEIGHT  41

int main(int argc, char **argv)
{
	printf("----------------\n");
	printf("VPO TEST PROGRAM\n");
	printf("----------------\n");
	printf("\n");

	if (argc < 5 ||
	    (strcmp (argv[1], "-h") == 0 ||
	     strcmp (argv[1], "--help") == 0 ||
	     strcmp (argv[1], "/?") == 0) )
	{
		printf("Usage: vpotest file.wad MAP01 x y angle\n");
		fflush(stdout);
		return 0;
	}

	const char *filename = argv[1];
	const char *map = argv[2];

	int x = atoi(argv[3]);
	int y = atoi(argv[4]);
	int angle = atoi(argv[5]);

	printf("Loading file: %s [%s]\n", filename, map);

	if (VPO_LoadWAD(filename) != 0)
	{
		printf("ERROR: %s\n", VPO_GetError());
		fflush(stdout);
		return 1;
	}

	if (VPO_OpenMap(map) != 0)
	{
		printf("ERROR: %s\n", VPO_GetError());
		fflush(stdout);
		VPO_FreeWAD();
		return 1;
	}

	int vp_num = VPO_TestSpot(TEST_VISPLANES, x, y, EYE_HEIGHT, angle);

	printf("\n");
	printf("Visplanes @ (%d %d) ---> %d\n", x, y, vp_num);
	fflush(stdout);

	VPO_CloseMap();
	VPO_FreeWAD();

	return 0;
}

#endif // VPO_TEST_PROGRAM

//--- editor settings ---
// vi:ts=4:sw=4:noexpandtab
