#pragma once
#include "pch.h"

namespace SoarCraft::QYun::TextureDecoder {
    using namespace System;

    typedef struct {
        Byte r;
        Byte g;
        Byte b;
        Byte a;
    } PVRTCTexelColor;

    typedef struct {
        int r;
        int g;
        int b;
        int a;
    } PVRTCTexelColorInt;

    typedef struct {
        PVRTCTexelColor a;
        PVRTCTexelColor b;
        Byte weight[32];
        UInt32 punch_through_flag;
    } PVRTCTexelInfo;
}
