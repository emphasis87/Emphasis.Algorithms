#ifndef TSource
#define TSource char
#endif

#ifndef TResult
#define TResult char
#endif

void kernel BilinearGray(
    global TSource* source,
    global TResult* result,
    int w0,
    int h0,
    float widthRatio,
    float heightRatio
){
    int x1 = get_global_id(0);
    int y1 = get_global_id(1);
    int w1 = get_global_size(0);
    int d1 = y1 * w1 + x1;
    float fx0 = widthRatio * x1;
    float fy0 = heightRatio * y1;
    int x0 = (int) fx0;
    int y0 = (int) fy0;
    float fxdiff = fx0 - x0;
    float fydiff = fy0 - y0;
    int d0 = y0 * w0 + x0;
    TSource s0 = source[d0];
    TSource s1 = source[d0 + 1];
    TSource s2 = source[d0 + w0];
    TSource s3 = source[d0 + w0 + 1];
    TResult gray = (TResult)(
        s0 * (1 - fxdiff) * (1 - fydiff) +
        s1 * fxdiff * (1 - fydiff) +
        s2 * (1 - fxdiff) * fydiff +
        s3 * fxdiff * fydiff
    );
    result[d1] = gray;
    //result[d1] = (TResult)source[d0];
}