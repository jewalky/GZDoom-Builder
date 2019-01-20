//------------------------------------------------------------------------
//  VPO_LIB : misc stuff
//------------------------------------------------------------------------
//
//  Copyright (C) 1993-1996 Id Software, Inc.
//  Copyright (C) 2005 Simon Howard
//  Copyright (C) 2012 Andrew Apted
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

namespace vpo
{


void I_Error (const char *error, ...)
{
	va_list	argptr;

	va_start(argptr, error);
	vfprintf(stderr, error, argptr);
	va_end(argptr);

	fprintf(stderr, "\n\n");
	fflush(stderr);

	exit(-1);
}


static int ClosestLine_CastingHoriz(fixed_t x, fixed_t y, int *side)
{
	int     best_match = -1;
	fixed_t best_dist  = 32000 << FRACBITS;

	for (int n = 0 ; n < numlines ; n++)
	{
		fixed_t ly1 = lines[n].v1->y;
		fixed_t ly2 = lines[n].v2->y;

		// ignore purely horizontal lines
		if (ly1 == ly2)
			continue;

		// does the linedef cross the horizontal ray?
		if ( (y < ly1) && (y < ly2) ) continue;
		if ( (y > ly1) && (y > ly2) ) continue;

		fixed_t lx1 = lines[n].v1->x;
		fixed_t lx2 = lines[n].v2->x;

		fixed_t quot = FixedDiv(y - ly1, ly2 - ly1);
		fixed_t dist = lx1 - x + FixedMul(lx2 - lx1, quot);

		if (abs(dist) < best_dist)
		{
			best_match = n;
			best_dist  = abs(dist);

			if (side)
			{
				if (best_dist < FRACUNIT / 8)
					*side = 0;  // on the line
				else if ( (ly1 > ly2) == (dist > 0))
					*side = 1;  // right side
				else
					*side = -1; // left side
			}
		}
	}

	return best_match;
}


sector_t * X_SectorForPoint(fixed_t x, fixed_t y)
{
	/* hack, hack...  I look for the first LineDef crossing
	   an horizontal half-line drawn from the cursor */

	int sd = 0;
	int ld = ClosestLine_CastingHoriz(x, y, &sd);

	// VOID checks
	if (ld < 0)
		return NULL;

	if (lines[ld].sidenum[sd <= 0 ? 1 : 0] < 0)
		return NULL;
	
	// get sector as DOOM would
	// (need this to handle self-referencing linedef tricks)
	subsector_t *sub = R_PointInSubsector(x, y); 

	return sub->sector;
}


} // namespace vpo

//--- editor settings ---
// vi:ts=4:sw=4:noexpandtab
