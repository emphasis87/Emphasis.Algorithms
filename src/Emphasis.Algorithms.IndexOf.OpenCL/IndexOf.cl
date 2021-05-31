#pragma OPENCL EXTENSION cl_khr_global_int32_base_atomics : enable
#pragma OPENCL EXTENSION cl_khr_local_int32_base_atomics : enable

#ifndef 
#define OPERATION ==
#endif

void kernel IndexOf(
    global int* source,
    global int* result,
    local int* temp,
    global int comparand,
){
    const int x = get_global_id(0);
    const int y = get_global_id(1);

    
}