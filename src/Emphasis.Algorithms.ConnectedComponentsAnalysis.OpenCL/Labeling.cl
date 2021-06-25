﻿#ifndef TSource
#define TSource int
#endif

#ifndef TResult
#define TResult int
#endif

#ifndef Expression
#define Expression a == b
#endif

void kernel Labeling4(
    global TSource* source,
    global TResult* source
){
    int x = get_global_id(0);
    int y = get_global_id(1);
    int w = get_global_size(0);
    int h = get_global_size(1);
    int d = x + y * w;

    int d01 = x + (y-1) * w;
    int d10 = (x-1) + y * w;
    int d12 = (x+1) + y * w;
    int d21 = x + (y+1) * w;

    source[d] = Expression;
}

void kernel Labeling8(
    global TSource* source,
    global TResult* source
){
    int x = get_global_id(0);
    int y = get_global_id(1);
    int w = get_global_size(0);
    int h = get_global_size(1);
    int d = x + y * w;

    int d00 = (x-1) + (y-1) * w;
    int d01 = x + (y-1) * w;
    int d02 = (x+1) + (y-1) * w;
    int d10 = (x-1) + y * w;
    int d12 = (x+1) + y * w;
    int d20 = (x-1) + (y+1) * w;
    int d21 = x + (y+1) * w;
    int d22 = (x+1) + (y+1) * w;

    source[d] = Expression;
}