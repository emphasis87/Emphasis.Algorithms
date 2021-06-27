#ifndef TSource
#define TSource int
#endif

#ifndef TResult
#define TResult int
#endif

#ifndef Expression
#define Expression (a==b)&&(a>0)
#endif

int2 constant N4[4] = { (int2)(0,-1), (int2)(-1,0), (int2)(1,0), (int2)(0,1) };
int2 constant N8[8] = { (int2)(-1,-1), (int2)(0,-1), (int2)(1,-1), (int2)(-1,0), (int2)(1,0), (int2)(-1,1), (int2)(0,1), (int2)(1,1) };

inline int2 GetNeighbour(int count, int i)
{
    switch(count)
    {
        case 4 : return N4[i];
        case 8 : return N8[i];
    }
}

void Labeling(
    global TSource* source,
    global TResult* result,
    global int* hasChanged,
    int neighbourCount
){
    int x = get_global_id(0);
    int y = get_global_id(1);
    int w = get_global_size(0);
    int h = get_global_size(1);
    int d = x + y * w;

    TSource a = source[d];

    for (int i = 0; i < neighbourCount; i++)
    {
        int2 p = GetNeighbour(neighbourCount, i);
        int px = x + p.x;
        int py = y + p.y;
        if (!(px < 0 || px >= w || py < 0 || py >= h))
        {
            int d1 = px + py * w;
            TSource b = source[d1];

            if (Expression)
            {
                TResult c0 = result[d];
                TResult c1 = result[d1];

                if (c0 > c1)
                {
                    TResult c = c1;
                
                    c = result[c];
                    c = result[c];
                    c = result[c];
                    c = result[c];
                
                    if (c < c0)
                    {
                        result[d] = c;
                        if (hasChanged[0] != 1)
                            hasChanged[0] = 1;
                    }
                }
            }
        }

        barrier(CLK_GLOBAL_MEM_FENCE);
    }
}

void kernel Labeling4(
    global TSource* source,
    global TResult* result,
    global int* hasChanged
){
    Labeling(source, result, hasChanged, 4);
}

void kernel Labeling8(
    global TSource* source,
    global TResult* result,
    global int* hasChanged
){
    Labeling(source, result, hasChanged, 8);
}