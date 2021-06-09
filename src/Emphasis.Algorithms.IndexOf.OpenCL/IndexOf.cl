#if TCounter == int
#pragma OPENCL EXTENSION cl_khr_global_int32_base_atomics : enable
#pragma OPENCL EXTENSION cl_khr_local_int32_base_atomics : enable
#endif

#if TCounter == long
#pragma OPENCL EXTENSION cl_khr_int64_base_atomics : enable
#endif

#ifndef TSource
#define TSource int
#endif

#ifndef TResult
#define TResult int
#endif

#ifndef Operation
#define Operation ==
#endif

void kernel IndexOf(
    global TSource* source,
    global TResult* result,
    global TCounter* counter,
    // local TResult* temp,
    TSource comparand
){
    int x = get_global_id(0);
    int y = get_global_id(1);
    int w = get_global_size(0);
    int lx = get_local_id(0);
    int lw = get_local_size(0);

    int d = x + y * w;
    TSource v = source[d];

    // Initialize the global counter
    if (x == 0 && y == 0){
        counter[0] = 0;
    }
    barrier(CLK_GLOBAL_MEM_FENCE);

    if (v Operation comparand){
        TCounter cnt = atomic_inc(&counter[0]);
        result[2 * cnt] = x;
        result[2 * cnt + 1] = y;
    }

    /*
    // Initialize the local counter
    local TCounter local_counter[2];
    if (lx == 0){
        local_counter[0] = 0;
    }
    barrier(CLK_LOCAL_MEM_FENCE);
    
    // Compare the value and store its index
    if (v Operation comparand){
        TCounter cnt = atomic_inc(&local_counter[0]);
        temp[2 * cnt] = x;
        temp[2 * cnt + 1] = y;
    }
    barrier(CLK_LOCAL_MEM_FENCE);

    // Check the number of indexes
    if (local_counter[0] == 0)
        return;

    // Sum the global count
    if (lx == 0){
        local_counter[1] = atomic_add(counter, local_counter[0]);
    }
    barrier(CLK_LOCAL_MEM_FENCE);

    // Copy local indexes to the result
    event_t copyEventId = async_work_group_copy(result + local_counter[1] * 2, temp, local_counter[0] * 2, 0);
    wait_group_events(1, &copyEventId);
    */
}