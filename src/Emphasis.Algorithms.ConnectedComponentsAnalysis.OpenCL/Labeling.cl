#ifndef TSource
#define TSource int
#endif

#ifndef TResult
#define TResult int
#endif

#ifndef Expression
#define Expression a == b
#endif

int2 constant N4[4] = { (0,-1), (-1,0), (1,0), (0,1) };
int2 constant N8[8] = { (-1,-1), (0,-1), (1,-1), (-1,0), (1,0), (-1,1), (0,1), (1,1) };

void kernel Labeling4(
    global TSource* source,
    global TResult* result,
    global int* hasChanged
){
    int x = get_global_id(0);
    int y = get_global_id(1);
    int w = get_global_size(0);
    int h = get_global_size(1);
    int d = x + y * w;

    TSource a = source[d];
    for (int i = 0; i < 4; i++)
    {
        int2 p = N4[i];
        int px = p.x;
        int py = p.y;
        if (!(px < 0 || px >= w || py < 0 || py >= h))
        {
            int pd = px + py * w;
            TSource b = source[pd];
            if (Expression)
            {
                TResult r0 = result[d];
                TResult r1 = result[pd];
                if (r0 > r1)
                {
                    TResult c = result[r0];
                    c = result[c];
                    c = result[c];
                    c = result[c];
                    if (r0 != c)
                    {
                        result[d] = c;
                        if (hasChanged[0] != 1)
                            hasChanged[0] = 1;
                    }
                }
            }
        }
    }
}

void kernel Labeling8(
    global TSource* source,
    global TResult* result,
    global int* hasChanged
){
    
}