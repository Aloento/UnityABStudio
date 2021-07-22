#include "pch.h"
#include "TextureDecoder.h"
#include "ASTC.h"
#include "ETC.h"

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeASTC(FastArgs, int32_t blockWidth, int32_t blockHeight) {
        const long num_blocks_x = (width + blockWidth - 1) / blockWidth;
        const long num_blocks_y = (height + blockHeight - 1) / blockHeight;

        Byte* d = Array2Ptr(data);
        UInt32* i = Array2UIntPtr(image);
        uint32_t buffer[144];

        for (long by = 0; by < num_blocks_y; by++) {
            for (long bx = 0; bx < num_blocks_x; bx++, d += 16) {
                DecodeBlock(d, blockWidth, blockHeight, buffer);
                CopyBlockBuffer(bx, by, width, height, blockWidth, blockHeight, buffer, i);
            }
        }
        return 1;
    }

    void TextureDecoderService::DecodeBlock(const uint8_t* buf, const int bw, const int bh, uint32_t* outbuf) {
        if (buf[0] == 0xfc && (buf[1] & 1) == 1) {
            uint_fast32_t c;
            if (buf[1] & 2)
                c = Color(f16ptr_to_u8(buf + 8), f16ptr_to_u8(buf + 10), f16ptr_to_u8(buf + 12), f16ptr_to_u8(buf + 14));
            else
                c = Color(buf[9], buf[11], buf[13], buf[15]);
            for (int i = 0; i < bw * bh; i++)
                outbuf[i] = c;
        }
        else if (((buf[0] & 0xc3) == 0xc0 && (buf[1] & 1) == 1) || (buf[0] & 0xf) == 0) {
            uint_fast32_t c = Color(255, 0, 255, 255);
            for (int i = 0; i < bw * bh; i++)
                outbuf[i] = c;
        }
        else {
            BlockData block_data;
            block_data.bw = bw;
            block_data.bh = bh;
            DecodeBlockParams(buf, &block_data);
            DecodeEndpoints(buf, &block_data);
            DecodeWeights(buf, &block_data);
            if (block_data.part_num > 1)
                select_partition(buf, &block_data);
            ApplicateColor(&block_data, outbuf);
        }
    }

    void TextureDecoderService::ApplicateColor(const BlockData* data, uint32_t* outbuf) {
        static const t_select_folor_func_ptr FuncTableC[] = {
          select_color, select_color,     select_color_hdr, select_color_hdr, select_color, select_color,
          select_color, select_color_hdr, select_color,     select_color,     select_color, select_color_hdr,
          select_color, select_color,     select_color_hdr, select_color_hdr };
        static const t_select_folor_func_ptr FuncTableA[] = {
          select_color, select_color,     select_color_hdr, select_color_hdr, select_color, select_color,
          select_color, select_color_hdr, select_color,     select_color,     select_color, select_color_hdr,
          select_color, select_color,     select_color,     select_color_hdr };
        if (data->dual_plane) {
            int ps[] = { 0, 0, 0, 0 };
            ps[data->plane_selector] = 1;
            if (data->part_num > 1) {
                for (int i = 0; i < data->bw * data->bh; i++) {
                    int p = data->partition[i];
                    uint_fast8_t r =
                        FuncTableC[data->cem[p]](data->endpoints[p][0], data->endpoints[p][4], data->weights[i][ps[0]]);
                    uint_fast8_t g =
                        FuncTableC[data->cem[p]](data->endpoints[p][1], data->endpoints[p][5], data->weights[i][ps[1]]);
                    uint_fast8_t b =
                        FuncTableC[data->cem[p]](data->endpoints[p][2], data->endpoints[p][6], data->weights[i][ps[2]]);
                    uint_fast8_t a =
                        FuncTableA[data->cem[p]](data->endpoints[p][3], data->endpoints[p][7], data->weights[i][ps[3]]);
                    outbuf[i] = Color(r, g, b, a);
                }
            }
            else {
                for (int i = 0; i < data->bw * data->bh; i++) {
                    uint_fast8_t r =
                        FuncTableC[data->cem[0]](data->endpoints[0][0], data->endpoints[0][4], data->weights[i][ps[0]]);
                    uint_fast8_t g =
                        FuncTableC[data->cem[0]](data->endpoints[0][1], data->endpoints[0][5], data->weights[i][ps[1]]);
                    uint_fast8_t b =
                        FuncTableC[data->cem[0]](data->endpoints[0][2], data->endpoints[0][6], data->weights[i][ps[2]]);
                    uint_fast8_t a =
                        FuncTableA[data->cem[0]](data->endpoints[0][3], data->endpoints[0][7], data->weights[i][ps[3]]);
                    outbuf[i] = Color(r, g, b, a);
                }
            }
        }
        else if (data->part_num > 1) {
            for (int i = 0; i < data->bw * data->bh; i++) {
                int p = data->partition[i];
                uint_fast8_t r =
                    FuncTableC[data->cem[p]](data->endpoints[p][0], data->endpoints[p][4], data->weights[i][0]);
                uint_fast8_t g =
                    FuncTableC[data->cem[p]](data->endpoints[p][1], data->endpoints[p][5], data->weights[i][0]);
                uint_fast8_t b =
                    FuncTableC[data->cem[p]](data->endpoints[p][2], data->endpoints[p][6], data->weights[i][0]);
                uint_fast8_t a =
                    FuncTableA[data->cem[p]](data->endpoints[p][3], data->endpoints[p][7], data->weights[i][0]);
                outbuf[i] = Color(r, g, b, a);
            }
        }
        else {
            for (int i = 0; i < data->bw * data->bh; i++) {
                uint_fast8_t r =
                    FuncTableC[data->cem[0]](data->endpoints[0][0], data->endpoints[0][4], data->weights[i][0]);
                uint_fast8_t g =
                    FuncTableC[data->cem[0]](data->endpoints[0][1], data->endpoints[0][5], data->weights[i][0]);
                uint_fast8_t b =
                    FuncTableC[data->cem[0]](data->endpoints[0][2], data->endpoints[0][6], data->weights[i][0]);
                uint_fast8_t a =
                    FuncTableA[data->cem[0]](data->endpoints[0][3], data->endpoints[0][7], data->weights[i][0]);
                outbuf[i] = Color(r, g, b, a);
            }
        }
    }

    void TextureDecoderService::DecodeWeights(const uint8_t* buf, BlockData* data) {
        IntSeqData seq[128];
        int wv[128] = {};
        decode_intseq(buf, 128, WeightPrecTableA[data->weight_range], WeightPrecTableB[data->weight_range],
            data->weight_num, 1, seq);

        if (WeightPrecTableA[data->weight_range] == 0) {
            switch (WeightPrecTableB[data->weight_range]) {
            case 1:
                for (int i = 0; i < data->weight_num; i++)
                    wv[i] = seq[i].bits ? 63 : 0;
                break;
            case 2:
                for (int i = 0; i < data->weight_num; i++)
                    wv[i] = seq[i].bits << 4 | seq[i].bits << 2 | seq[i].bits;
                break;
            case 3:
                for (int i = 0; i < data->weight_num; i++)
                    wv[i] = seq[i].bits << 3 | seq[i].bits;
                break;
            case 4:
                for (int i = 0; i < data->weight_num; i++)
                    wv[i] = seq[i].bits << 2 | seq[i].bits >> 2;
                break;
            case 5:
                for (int i = 0; i < data->weight_num; i++)
                    wv[i] = seq[i].bits << 1 | seq[i].bits >> 4;
                break;
            }
            for (int i = 0; i < data->weight_num; i++)
                if (wv[i] > 32)
                    ++wv[i];
        }
        else if (WeightPrecTableB[data->weight_range] == 0) {
            int s = WeightPrecTableA[data->weight_range] == 3 ? 32 : 16;
            for (int i = 0; i < data->weight_num; i++)
                wv[i] = seq[i].nonbits * s;
        }
        else {
            if (WeightPrecTableA[data->weight_range] == 3) {
                switch (WeightPrecTableB[data->weight_range]) {
                case 1:
                    for (int i = 0; i < data->weight_num; i++)
                        wv[i] = seq[i].nonbits * 50;
                    break;
                case 2:
                    for (int i = 0; i < data->weight_num; i++) {
                        wv[i] = seq[i].nonbits * 23;
                        if (seq[i].bits & 2)
                            wv[i] += 0b1000101;
                    }
                    break;
                case 3:
                    for (int i = 0; i < data->weight_num; i++)
                        wv[i] = seq[i].nonbits * 11 + ((seq[i].bits << 4 | seq[i].bits >> 1) & 0b1100011);
                    break;
                }
            }
            else if (WeightPrecTableA[data->weight_range] == 5) {
                switch (WeightPrecTableB[data->weight_range]) {
                case 1:
                    for (int i = 0; i < data->weight_num; i++)
                        wv[i] = seq[i].nonbits * 28;
                    break;
                case 2:
                    for (int i = 0; i < data->weight_num; i++) {
                        wv[i] = seq[i].nonbits * 13;
                        if (seq[i].bits & 2)
                            wv[i] += 0b1000010;
                    }
                    break;
                }
            }
            for (int i = 0; i < data->weight_num; i++) {
                int a = (seq[i].bits & 1) * 0x7f;
                wv[i] = (a & 0x20) | ((wv[i] ^ a) >> 2);
                if (wv[i] > 32)
                    ++wv[i];
            }
        }

        int ds = (1024 + data->bw / 2) / (data->bw - 1);
        int dt = (1024 + data->bh / 2) / (data->bh - 1);
        int pn = data->dual_plane ? 2 : 1;

        for (int t = 0, i = 0; t < data->bh; t++) {
            for (int s = 0; s < data->bw; s++, i++) {
                int gs = (ds * s * (data->width - 1) + 32) >> 6;
                int gt = (dt * t * (data->height - 1) + 32) >> 6;
                int fs = gs & 0xf;
                int ft = gt & 0xf;
                int v = (gs >> 4) + (gt >> 4) * data->width;
                int w11 = (fs * ft + 8) >> 4;
                int w10 = ft - w11;
                int w01 = fs - w11;
                int w00 = 16 - fs - ft + w11;

                for (int p = 0; p < pn; p++) {
                    int p00 = wv[v * pn + p];
                    int p01 = wv[(v + 1) * pn + p];
                    int p10 = wv[(v + data->width) * pn + p];
                    int p11 = wv[(v + data->width + 1) * pn + p];
                    data->weights[i][p] = (p00 * w00 + p01 * w01 + p10 * w10 + p11 * w11 + 8) >> 4;
                }
            }
        }
    }

    void TextureDecoderService::DecodeEndpoints(const uint8_t* buf, BlockData* data) {
        static const int TritsTable[] = { 0, 204, 93, 44, 22, 11, 5 };
        static const int QuintsTable[] = { 0, 113, 54, 26, 13, 6 };
        IntSeqData seq[32];
        int ev[32];
        decode_intseq(buf, data->part_num == 1 ? 17 : 29, CemTableA[data->cem_range], CemTableB[data->cem_range],
            data->endpoint_value_num, 0, seq);

        switch (CemTableA[data->cem_range]) {
        case 3:
            for (int i = 0, b, c = TritsTable[CemTableB[data->cem_range]]; i < data->endpoint_value_num; i++) {
                int a = (seq[i].bits & 1) * 0x1ff;
                int x = seq[i].bits >> 1;
                switch (CemTableB[data->cem_range]) {
                case 1:
                    b = 0;
                    break;
                case 2:
                    b = 0b100010110 * x;
                    break;
                case 3:
                    b = x << 7 | x << 2 | x;
                    break;
                case 4:
                    b = x << 6 | x;
                    break;
                case 5:
                    b = x << 5 | x >> 2;
                    break;
                case 6:
                    b = x << 4 | x >> 4;
                    break;
                }
                ev[i] = (a & 0x80) | ((seq[i].nonbits * c + b) ^ a) >> 2;
            }
            break;
        case 5:
            for (int i = 0, b, c = QuintsTable[CemTableB[data->cem_range]]; i < data->endpoint_value_num; i++) {
                int a = (seq[i].bits & 1) * 0x1ff;
                int x = seq[i].bits >> 1;
                switch (CemTableB[data->cem_range]) {
                case 1:
                    b = 0;
                    break;
                case 2:
                    b = 0b100001100 * x;
                    break;
                case 3:
                    b = x << 7 | x << 1 | x >> 1;
                    break;
                case 4:
                    b = x << 6 | x >> 1;
                    break;
                case 5:
                    b = x << 5 | x >> 3;
                    break;
                }
                ev[i] = (a & 0x80) | ((seq[i].nonbits * c + b) ^ a) >> 2;
            }
            break;
        default:
            switch (CemTableB[data->cem_range]) {
            case 1:
                for (int i = 0; i < data->endpoint_value_num; i++)
                    ev[i] = seq[i].bits * 0xff;
                break;
            case 2:
                for (int i = 0; i < data->endpoint_value_num; i++)
                    ev[i] = seq[i].bits * 0x55;
                break;
            case 3:
                for (int i = 0; i < data->endpoint_value_num; i++)
                    ev[i] = seq[i].bits << 5 | seq[i].bits << 2 | seq[i].bits >> 1;
                break;
            case 4:
                for (int i = 0; i < data->endpoint_value_num; i++)
                    ev[i] = seq[i].bits << 4 | seq[i].bits;
                break;
            case 5:
                for (int i = 0; i < data->endpoint_value_num; i++)
                    ev[i] = seq[i].bits << 3 | seq[i].bits >> 2;
                break;
            case 6:
                for (int i = 0; i < data->endpoint_value_num; i++)
                    ev[i] = seq[i].bits << 2 | seq[i].bits >> 4;
                break;
            case 7:
                for (int i = 0; i < data->endpoint_value_num; i++)
                    ev[i] = seq[i].bits << 1 | seq[i].bits >> 6;
                break;
            case 8:
                for (int i = 0; i < data->endpoint_value_num; i++)
                    ev[i] = seq[i].bits;
                break;
            }
        }

        int* v = ev;
        for (int cem = 0; cem < data->part_num; v += (data->cem[cem] / 4 + 1) * 2, cem++) {
            switch (data->cem[cem]) {
            case 0:
                set_endpoint(data->endpoints[cem], v[0], v[0], v[0], 255, v[1], v[1], v[1], 255);
                break;
            case 1: {
                int l0 = (v[0] >> 2) | (v[1] & 0xc0);
                int l1 = Clamp(l0 + (v[1] & 0x3f));
                set_endpoint(data->endpoints[cem], l0, l0, l0, 255, l1, l1, l1, 255);
            } break;
            case 2: {
                int y0, y1;
                if (v[0] <= v[1]) {
                    y0 = v[0] << 4;
                    y1 = v[1] << 4;
                }
                else {
                    y0 = (v[1] << 4) + 8;
                    y1 = (v[0] << 4) - 8;
                }
                set_endpoint_hdr(data->endpoints[cem], y0, y0, y0, 0x780, y1, y1, y1, 0x780);
            } break;
            case 3: {
                int y0, d;
                if (v[0] & 0x80) {
                    y0 = (v[1] & 0xe0) << 4 | (v[0] & 0x7f) << 2;
                    d = (v[1] & 0x1f) << 2;
                }
                else {
                    y0 = (v[1] & 0xf0) << 4 | (v[0] & 0x7f) << 1;
                    d = (v[1] & 0x0f) << 1;
                }
                int y1 = clamp_hdr(y0 + d);
                set_endpoint_hdr(data->endpoints[cem], y0, y0, y0, 0x780, y1, y1, y1, 0x780);
            } break;
            case 4:
                set_endpoint(data->endpoints[cem], v[0], v[0], v[0], v[2], v[1], v[1], v[1], v[3]);
                break;
            case 5:
                bit_transfer_signed(&v[1], &v[0]);
                bit_transfer_signed(&v[3], &v[2]);
                v[1] += v[0];
                set_endpoint_clamp(data->endpoints[cem], v[0], v[0], v[0], v[2], v[1], v[1], v[1], v[2] + v[3]);
                break;
            case 6:
                set_endpoint(data->endpoints[cem], v[0] * v[3] >> 8, v[1] * v[3] >> 8, v[2] * v[3] >> 8, 255, v[0], v[1],
                    v[2], 255);
                break;
            case 7:
                decode_endpoints_hdr7(data->endpoints[cem], v);
                break;
            case 8:
                if (v[0] + v[2] + v[4] <= v[1] + v[3] + v[5])
                    set_endpoint(data->endpoints[cem], v[0], v[2], v[4], 255, v[1], v[3], v[5], 255);
                else
                    set_endpoint_blue(data->endpoints[cem], v[1], v[3], v[5], 255, v[0], v[2], v[4], 255);
                break;
            case 9:
                bit_transfer_signed(&v[1], &v[0]);
                bit_transfer_signed(&v[3], &v[2]);
                bit_transfer_signed(&v[5], &v[4]);
                if (v[1] + v[3] + v[5] >= 0)
                    set_endpoint_clamp(data->endpoints[cem], v[0], v[2], v[4], 255, v[0] + v[1], v[2] + v[3], v[4] + v[5],
                        255);
                else
                    set_endpoint_blue_clamp(data->endpoints[cem], v[0] + v[1], v[2] + v[3], v[4] + v[5], 255, v[0], v[2],
                        v[4], 255);
                break;
            case 10:
                set_endpoint(data->endpoints[cem], v[0] * v[3] >> 8, v[1] * v[3] >> 8, v[2] * v[3] >> 8, v[4], v[0], v[1],
                    v[2], v[5]);
                break;
            case 11:
                decode_endpoints_hdr11(data->endpoints[cem], v, 0x780, 0x780);
                break;
            case 12:
                if (v[0] + v[2] + v[4] <= v[1] + v[3] + v[5])
                    set_endpoint(data->endpoints[cem], v[0], v[2], v[4], v[6], v[1], v[3], v[5], v[7]);
                else
                    set_endpoint_blue(data->endpoints[cem], v[1], v[3], v[5], v[7], v[0], v[2], v[4], v[6]);
                break;
            case 13:
                bit_transfer_signed(&v[1], &v[0]);
                bit_transfer_signed(&v[3], &v[2]);
                bit_transfer_signed(&v[5], &v[4]);
                bit_transfer_signed(&v[7], &v[6]);
                if (v[1] + v[3] + v[5] >= 0)
                    set_endpoint_clamp(data->endpoints[cem], v[0], v[2], v[4], v[6], v[0] + v[1], v[2] + v[3], v[4] + v[5],
                        v[6] + v[7]);
                else
                    set_endpoint_blue_clamp(data->endpoints[cem], v[0] + v[1], v[2] + v[3], v[4] + v[5], v[6] + v[7], v[0],
                        v[2], v[4], v[6]);
                break;
            case 14:
                decode_endpoints_hdr11(data->endpoints[cem], v, v[6], v[7]);
                break;
            case 15: {
                int mode = ((v[6] >> 7) & 1) | ((v[7] >> 6) & 2);
                v[6] &= 0x7f;
                v[7] &= 0x7f;
                if (mode == 3) {
                    decode_endpoints_hdr11(data->endpoints[cem], v, v[6] << 5, v[7] << 5);
                }
                else {
                    v[6] |= (v[7] << (mode + 1)) & 0x780;
                    v[7] = ((v[7] & (0x3f >> mode)) ^ (0x20 >> mode)) - (0x20 >> mode);
                    v[6] <<= 4 - mode;
                    v[7] <<= 4 - mode;
                    decode_endpoints_hdr11(data->endpoints[cem], v, v[6], clamp_hdr(v[6] + v[7]));
                }
            } break;
                //default:
                //    rb_raise(rb_eStandardError, "Unsupported ASTC format");
            }
        }
    }

    void TextureDecoderService::DecodeBlockParams(const uint8_t* buf, BlockData* block_data) {
        block_data->dual_plane = !!(buf[1] & 4);
        block_data->weight_range = (buf[0] >> 4 & 1) | (buf[1] << 2 & 8);

        if (buf[0] & 3) {
            block_data->weight_range |= buf[0] << 1 & 6;
            switch (buf[0] & 0xc) {
            case 0:
                block_data->width = (u8ptr_to_u16(buf) >> 7 & 3) + 4;
                block_data->height = (buf[0] >> 5 & 3) + 2;
                break;
            case 4:
                block_data->width = (u8ptr_to_u16(buf) >> 7 & 3) + 8;
                block_data->height = (buf[0] >> 5 & 3) + 2;
                break;
            case 8:
                block_data->width = (buf[0] >> 5 & 3) + 2;
                block_data->height = (u8ptr_to_u16(buf) >> 7 & 3) + 8;
                break;
            case 12:
                if (buf[1] & 1) {
                    block_data->width = (buf[0] >> 7 & 1) + 2;
                    block_data->height = (buf[0] >> 5 & 3) + 2;
                }
                else {
                    block_data->width = (buf[0] >> 5 & 3) + 2;
                    block_data->height = (buf[0] >> 7 & 1) + 6;
                }
                break;
            }
        }
        else {
            block_data->weight_range |= buf[0] >> 1 & 6;
            switch (u8ptr_to_u16(buf) & 0x180) {
            case 0:
                block_data->width = 12;
                block_data->height = (buf[0] >> 5 & 3) + 2;
                break;
            case 0x80:
                block_data->width = (buf[0] >> 5 & 3) + 2;
                block_data->height = 12;
                break;
            case 0x100:
                block_data->width = (buf[0] >> 5 & 3) + 6;
                block_data->height = (buf[1] >> 1 & 3) + 6;
                block_data->dual_plane = 0;
                block_data->weight_range &= 7;
                break;
            case 0x180:
                block_data->width = (buf[0] & 0x20) ? 10 : 6;
                block_data->height = (buf[0] & 0x20) ? 6 : 10;
                break;
            }
        }

        block_data->part_num = (buf[1] >> 3 & 3) + 1;

        block_data->weight_num = block_data->width * block_data->height;
        if (block_data->dual_plane)
            block_data->weight_num *= 2;

        int weight_bits, config_bits, cem_base = 0;

        switch (WeightPrecTableA[block_data->weight_range]) {
        case 3:
            weight_bits =
                block_data->weight_num * WeightPrecTableB[block_data->weight_range] + (block_data->weight_num * 8 + 4) / 5;
            break;
        case 5:
            weight_bits =
                block_data->weight_num * WeightPrecTableB[block_data->weight_range] + (block_data->weight_num * 7 + 2) / 3;
            break;
        default:
            weight_bits = block_data->weight_num * WeightPrecTableB[block_data->weight_range];
        }

        if (block_data->part_num == 1) {
            block_data->cem[0] = u8ptr_to_u16(buf + 1) >> 5 & 0xf;
            config_bits = 17;
        }
        else {
            cem_base = u8ptr_to_u16(buf + 2) >> 7 & 3;
            if (cem_base == 0) {
                int cem = buf[3] >> 1 & 0xf;
                for (int i = 0; i < block_data->part_num; i++)
                    block_data->cem[i] = cem;
                config_bits = 29;
            }
            else {
                for (int i = 0; i < block_data->part_num; i++)
                    block_data->cem[i] = ((buf[3] >> (i + 1) & 1) + cem_base - 1) << 2;
                switch (block_data->part_num) {
                case 2:
                    block_data->cem[0] |= buf[3] >> 3 & 3;
                    block_data->cem[1] |= getbits(buf, 126 - weight_bits, 2);
                    break;
                case 3:
                    block_data->cem[0] |= buf[3] >> 4 & 1;
                    block_data->cem[0] |= getbits(buf, 122 - weight_bits, 2) & 2;
                    block_data->cem[1] |= getbits(buf, 124 - weight_bits, 2);
                    block_data->cem[2] |= getbits(buf, 126 - weight_bits, 2);
                    break;
                case 4:
                    for (int i = 0; i < 4; i++)
                        block_data->cem[i] |= getbits(buf, 120 + i * 2 - weight_bits, 2);
                    break;
                }
                config_bits = 25 + block_data->part_num * 3;
            }
        }

        if (block_data->dual_plane) {
            config_bits += 2;
            block_data->plane_selector =
                getbits(buf, cem_base ? 130 - weight_bits - block_data->part_num * 3 : 126 - weight_bits, 2);
        }

        int remain_bits = 128 - config_bits - weight_bits;

        block_data->endpoint_value_num = 0;
        for (int i = 0; i < block_data->part_num; i++)
            block_data->endpoint_value_num += (block_data->cem[i] >> 1 & 6) + 2;

        for (int i = 0, endpoint_bits; i < (int)(sizeof(CemTableA) / sizeof(int)); i++) {
            switch (CemTableA[i]) {
            case 3:
                endpoint_bits =
                    block_data->endpoint_value_num * CemTableB[i] + (block_data->endpoint_value_num * 8 + 4) / 5;
                break;
            case 5:
                endpoint_bits =
                    block_data->endpoint_value_num * CemTableB[i] + (block_data->endpoint_value_num * 7 + 2) / 3;
                break;
            default:
                endpoint_bits = block_data->endpoint_value_num * CemTableB[i];
            }

            if (endpoint_bits <= remain_bits) {
                block_data->cem_range = i;
                break;
            }
        }
    }
}
