#ifndef TSource
#define TSource int
#endif

#ifndef Expression
#define Expression d
#endif

void kernel Formula(
    global TSource* source
){
    int x = get_global_id(0);
    int y = get_global_id(1);
    int w = get_global_size(0);
    int h = get_global_size(1);
    int d = x + y * w;

    source[d] = Expression;
}