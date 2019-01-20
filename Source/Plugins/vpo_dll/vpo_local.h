//------------------------------------------------------------------------
//  Visplane Overflow Library
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

#ifndef __VPO_LOCAL_H__
#define __VPO_LOCAL_H__

#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <string.h>
#include <math.h>

#include "sys_type.h"
#include "sys_macro.h"
#include "sys_endian.h"

namespace vpo
{

#include "doomtype.h"
#include "doomdef.h"
#include "doomdata.h"

#include "m_bbox.h"
#include "m_fixed.h"

#include "w_file.h"
#include "w_wad.h"

#include "tables.h"
#include "r_defs.h"
#include "r_state.h"

#include "r_main.h"
#include "r_bsp.h"
#include "r_plane.h"


//------------------------------------------------------------

#define SHORT(x)  LE_S16(x)
#define LONG(x)   LE_S32(x)

const char * P_SetupLevel (const char *lumpname, bool *is_hexen );
void P_FreeLevelData (void);

void I_Error (const char *error, ...);

sector_t * X_SectorForPoint(fixed_t x, fixed_t y);

// exceptions thrown on overflows
class overflow_exception { };

} // namespace vpo


#endif  /* __VPO_LOCAL_H__ */

//--- editor settings ---
// vi:ts=4:sw=4:noexpandtab
