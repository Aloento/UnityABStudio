#include "pch.h"
#include "crunch.h"
#include <stdint.h>
#include <algorithm>
#include "crunch/crn_decomp.h"

bool crunch_unpack_level(const uint8_t* data, uint32_t data_size, uint32_t level_index, void** ret, uint32_t* ret_size) {
	crnd::crn_texture_info tex_info;
	if (!crnd::crnd_get_texture_info(data, data_size, &tex_info))
	{
		return false;
	}

	crnd::crnd_unpack_context pContext = crnd::crnd_unpack_begin(data, data_size);
	if (!pContext)
	{
		return false;
	}

	const crn_uint32 width = std::max(1U, tex_info.m_width >> level_index);
	const crn_uint32 height = std::max(1U, tex_info.m_height >> level_index);
	const crn_uint32 blocks_x = std::max(1U, (width + 3) >> 2);
	const crn_uint32 blocks_y = std::max(1U, (height + 3) >> 2);
	const crn_uint32 row_pitch = blocks_x * crnd::crnd_get_bytes_per_dxt_block(tex_info.m_format);
	const crn_uint32 total_face_size = row_pitch * blocks_y;
	*ret = new uint8_t[total_face_size];
	*ret_size = total_face_size;
	if (!crnd::crnd_unpack_level(pContext, ret, total_face_size, row_pitch, level_index))
	{
		crnd::crnd_unpack_end(pContext);
		return false;
	}
	crnd::crnd_unpack_end(pContext);
	return true;
}
