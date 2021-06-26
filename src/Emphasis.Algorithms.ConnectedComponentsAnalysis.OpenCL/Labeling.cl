#ifndef TSource
#define TSource int
#endif

#ifndef TResult
#define TResult int
#endif

#ifndef Expression
#define Expression a == b
#endif

global static int2[] N4 = { (0,-1), (-1,0), (1,0), (0,1) };
global static int2[] N8 = { (-1,-1), (0,-1), (1,-1), (-1,0), (1,0), (-1,1), (0,1), (1,1) };

void kernel Labeling4(
    global TSource* source,
    global TResult* result
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
                    result[d] = c;
                }
            }
        }
    }

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