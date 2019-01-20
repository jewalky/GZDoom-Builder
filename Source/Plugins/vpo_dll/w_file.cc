// Emacs style mode select   -*- C++ -*- 
//-----------------------------------------------------------------------------
//
// Copyright(C) 1993-1996 Id Software, Inc.
// Copyright(C) 2008 Simon Howard
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

#include "vpo_local.h"

namespace vpo
{


wad_file_t *W_OpenFile(const char *path)
{
    wad_file_t *result;

    FILE *fstream = fopen(path, "rb");

    if (fstream == NULL)
    {
        return NULL;
    }

    // Create a new wad_file_t to hold the file handle.

    result = new wad_file_t;

    result->fstream = fstream;

//    result->length = M_FileLength(fstream);

    return result;
}


void W_CloseFile(wad_file_t *wad)
{
    fclose(wad->fstream);

    delete wad;
}


// Read data from the specified position in the file into the 
// provided buffer.  Returns the number of bytes read.

size_t W_Read(wad_file_t *wad, unsigned int offset,
              void *buffer, size_t buffer_len)
{
    size_t result;

    // Jump to the specified position in the file.

    fseek(wad->fstream, offset, SEEK_SET);

    // Read into the buffer.

    result = fread(buffer, 1, buffer_len, wad->fstream);

    return result;
}


} // namespace vpo

