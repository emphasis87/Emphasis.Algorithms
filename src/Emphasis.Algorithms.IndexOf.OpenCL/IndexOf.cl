#ifdef Atomics_32
#pragma OPENCL EXTENSION cl_khr_global_int32_base_atomics : enable
#pragma OPENCL EXTENSION cl_khr_local_int32_base_atomics : enable
#endif

#ifdef Atomics_64
#pragma OPENCL EXTENSION cl_khr_int64_base_atomics : enable
#endif

#ifndef TSourceDepth
#define TSourceDepth int
#endif

#ifndef TResultDepth
#define TResultDepth int
#endif

#ifndef Operation
#define Operation ==
#endif

void kernel IndexOf(
    global TSourceDepth* source,
    global TResultDepth* result,
    local TResultDepth* temp,
    int comparand
){
    int w = get_global_size(0);
    int x = get_global_id(0);
    int y = get_global_id(1);
    int d = y * w + x;
    TSourceDepth v = source[d];

    if (v Operation comparand){
        
    }
}