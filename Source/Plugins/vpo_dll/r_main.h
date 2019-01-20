// Emacs style mode select   -*- C++ -*- 
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
//	System specific interface stuff.
//
//-----------------------------------------------------------------------------

#ifndef __R_MAIN__
#define __R_MAIN__


//
// POV related.
//
extern fixed_t		viewcos;
extern fixed_t		viewsin;

extern int		viewwidth;
extern int		viewheight;
extern int		viewwindowx;
extern int		viewwindowy;


extern int		centerx;
extern int		centery;

extern fixed_t		centerxfrac;
extern fixed_t		centeryfrac;
extern fixed_t		projection;

extern int		validcount;

extern int		linecount;
extern int		loopcount;


//
// Lighting LUT.
// Used for z-depth cuing per column/row,
//  and other lighting effects (sector ambient, flash).
//

// Lighting constants.
// Now why not 32 levels here?
#define LIGHTLEVELS	        16
#define LIGHTSEGSHIFT	         4

#define MAXLIGHTSCALE		48
#define LIGHTSCALESHIFT		12
#define MAXLIGHTZ	       128
#define LIGHTZSHIFT		20


// Number of diminishing brightness levels.
// There a 0-31, i.e. 32 LUT in the COLORMAP lump.
#define NUMCOLORMAPS		32


//
// Utility functions.
int R_PointOnSide ( fixed_t	x, fixed_t	y, node_t*	node );

int R_PointOnSegSide ( fixed_t	x, fixed_t	y, seg_t*	line );

angle_t R_PointToAngle ( fixed_t	x, fixed_t	y );

angle_t R_PointToAngle2 ( fixed_t	x1, fixed_t	y1, fixed_t	x2, fixed_t	y2 );

fixed_t R_PointToDist ( fixed_t	x, fixed_t	y );


fixed_t R_ScaleFromGlobalAngle (angle_t visangle);

subsector_t* R_PointInSubsector ( fixed_t	x, fixed_t	y );

void R_AddPointToBox ( int		x, int		y, fixed_t*	box );



// R_THINGS

extern short  screenheightarray[SCREENWIDTH];
extern short  negonearray[SCREENWIDTH];


//
// REFRESH - the actual rendering functions.
//

void R_Init (void);

void R_RenderView (fixed_t x, fixed_t y, fixed_t z, angle_t angle);

#endif
