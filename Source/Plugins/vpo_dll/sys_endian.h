//------------------------------------------------------------------------
//  EDGE Endian handling
//------------------------------------------------------------------------
//
//  Eureka DOOM Editor
//
//  Copyright (C) 2006-2008 Andrew Apted
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
//
//  Using code from SDL_byteorder.h and SDL_endian.h.
//  Copyright (C) 1997-2004 Sam Lantinga.
//
//------------------------------------------------------------------------

#ifndef __SYS_ENDIAN_H__
#define __SYS_ENDIAN_H__


// ---- determine byte order ----

#define UT_LIL_ENDIAN  1234
#define UT_BIG_ENDIAN  4321

#if defined(__LITTLE_ENDIAN__) || defined(WIN32) ||  \
    defined(__i386__) || defined(__i386) ||          \
    defined(__ia64__) || defined(__x86_64__)  ||     \
    defined(__alpha__) || defined(__alpha)  ||       \
    defined(__arm__) || defined(__SYMBIAN32__) ||    \
    (defined(__mips__) && defined(__MIPSEL__))
#define UT_BYTEORDER   UT_LIL_ENDIAN
#else
#define UT_BYTEORDER   UT_BIG_ENDIAN
#endif


// ---- the gruntwork of swapping ----

#if defined(__GNUC__) && defined(__i386__)
static inline u16_t UT_Swap16(u16_t x)
{
  __asm__("xchgb %b0,%h0" : "=q" (x) :  "0" (x));
  return x;
}
#elif defined(__GNUC__) && defined(__x86_64__)
static inline u16_t UT_Swap16(u16_t x)
{
  __asm__("xchgb %b0,%h0" : "=Q" (x) :  "0" (x));
  return x;
}
#elif defined(__GNUC__) && (defined(__powerpc__) || defined(__ppc__))
static inline u16_t UT_Swap16(u16_t x)
{
  u16_t result;

  __asm__("rlwimi %0,%2,8,16,23" : "=&r" (result) : "0" (x >> 8), "r" (x));
  return result;
}
#else
static inline u16_t UT_Swap16(u16_t x) {
  return((x<<8)|(x>>8));
}
#endif

#if defined(__GNUC__) && defined(__i386__)
static inline u32_t UT_Swap32(u32_t x)
{
  __asm__("bswap %0" : "=r" (x) : "0" (x));
  return x;
}
#elif defined(__GNUC__) && defined(__x86_64__)
static inline u32_t UT_Swap32(u32_t x)
{
  __asm__("bswapl %0" : "=r" (x) : "0" (x));
  return x;
}
#elif defined(__GNUC__) && (defined(__powerpc__) || defined(__ppc__))
static inline u32_t UT_Swap32(u32_t x)
{
  u32_t result;

  __asm__("rlwimi %0,%2,24,16,23" : "=&r" (result) : "0" (x>>24), "r" (x));
  __asm__("rlwimi %0,%2,8,8,15"   : "=&r" (result) : "0" (result),    "r" (x));
  __asm__("rlwimi %0,%2,24,0,7"   : "=&r" (result) : "0" (result),    "r" (x));
  return result;
}
#else
static inline u32_t UT_Swap32(u32_t x) {
  return ((x<<24)|((x<<8)&0x00FF0000)|((x>>8)&0x0000FF00)|(x>>24));
}
#endif


// ---- byte swap from specified endianness to native ----

#if (UT_BYTEORDER == UT_LIL_ENDIAN)
#define LE_U16(X)  ((u16_t)(X))
#define LE_U32(X)  ((u32_t)(X))
#define BE_U16(X)  UT_Swap16(X)
#define BE_U32(X)  UT_Swap32(X)
#else
#define LE_U16(X)  UT_Swap16(X)
#define LE_U32(X)  UT_Swap32(X)
#define BE_U16(X)  ((u16_t)(X))
#define BE_U32(X)  ((u32_t)(X))
#endif

// signed versions of the above
#define LE_S16(X)  ((s16_t) LE_U16((u16_t) (X)))
#define LE_S32(X)  ((s32_t) LE_U32((u32_t) (X)))
#define BE_S16(X)  ((s16_t) BE_U16((u16_t) (X)))
#define BE_S32(X)  ((s32_t) BE_U32((u32_t) (X)))


#endif // __SYS_ENDIAN_H__

//--- editor settings ---
// vi:ts=4:sw=4:noexpandtab
