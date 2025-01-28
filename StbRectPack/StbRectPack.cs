namespace StbSharp;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using stbrp_coord = int;

public class StbRectPack
{
    // stb_rect_pack.h - v1.01 - public domain - rectangle packing
    // Sean Barrett 2014
    //
    // Useful for e.g. packing rectangular textures into an atlas.
    // Does not do rotation.
    //
    // Before #including,
    //
    //    #define STB_RECT_PACK_IMPLEMENTATION
    //
    // in the file that you want to have the implementation.
    //
    // Not necessarily the awesomest packing method, but better than
    // the totally naive one in stb_truetype (which is primarily what
    // this is meant to replace).
    //
    // Has only had a few tests run, may have issues.
    //
    // More docs to come.
    //
    // No memory allocations; uses qsort() and assert() from stdlib.
    // Can override those by defining STBRP_SORT and STBRP_ASSERT.
    //
    // This library currently uses the Skyline Bottom-Left algorithm.
    //
    // Please note: better rectangle packers are welcome! Please
    // implement them to the same API, but with a different init
    // function.
    //
    // Credits
    //
    //  Library
    //    Sean Barrett
    //  Minor features
    //    Martins Mozeiko
    //    github:IntellectualKitty
    //
    //  Bugfixes / warning fixes
    //    Jeremy Jaussaud
    //    Fabian Giesen
    //
    // Version history:
    //
    //     1.01  (2021-07-11)  always use large rect mode, expose STBRP__MAXVAL in public section
    //     1.00  (2019-02-25)  avoid small space waste; gracefully fail too-wide rectangles
    //     0.99  (2019-02-07)  warning fixes
    //     0.11  (2017-03-03)  return packing success/fail result
    //     0.10  (2016-10-25)  remove cast-away-const to avoid warnings
    //     0.09  (2016-08-27)  fix compiler warnings
    //     0.08  (2015-09-13)  really fix bug with empty rects (w=0 or h=0)
    //     0.07  (2015-09-13)  fix bug with empty rects (w=0 or h=0)
    //     0.06  (2015-04-15)  added STBRP_SORT to allow replacing qsort
    //     0.05:  added STBRP_ASSERT to allow replacing assert
    //     0.04:  fixed minor bug in STBRP_LARGE_RECTS support
    //     0.01:  initial release
    //
    // LICENSE
    //
    //   See end of file for license information.

    //////////////////////////////////////////////////////////////////////////////
    //
    //       INCLUDE SECTION
    //



    private const int STBRP__MAXVAL = 0x7fffffff;
    // Mostly for internal use, but this is the maximum supported coordinate value.

    // Assign packed locations to rectangles. The rectangles are of type
    // 'stbrp_rect' defined below, stored in the array 'rects', and there
    // are 'num_rects' many of them.
    //
    // Rectangles which are successfully packed have the 'was_packed' flag
    // set to a non-zero value and 'x' and 'y' store the minimum location
    // on each axis (i.e. bottom-left in cartesian coordinates, top-left
    // if you imagine y increasing downwards). Rectangles which do not fit
    // have the 'was_packed' flag set to 0.
    //
    // You should not try to access the 'rects' array from another thread
    // while this function is running, as the function temporarily reorders
    // the array while it executes.
    //
    // To pack into another rectangle, you need to call stbrp_init_target
    // again. To continue packing into the same rectangle, you can call
    // this function again. Calling this multiple times with multiple rect
    // arrays will probably produce worse packing results than calling it
    // a single time with the full rectangle array, but the option is
    // available.
    //
    // The function returns 1 if all of the rectangles were successfully
    // packed and 0 otherwise.
    static public int stbrp_pack_rects(ref stbrp_context context, stbrp_rect[] rects, int num_rects)
    {
        int i, all_rects_packed = 1;

        // we use the 'was_packed' field internally to allow sorting/unsorting
        for (i = 0; i < num_rects; ++i)
        {
            rects[i].was_packed = i;
        }

        // sort according to heuristic

        Array.Sort(rects, (a, b) => rect_height_compare(ref a, ref b));

        for (i = 0; i < num_rects; ++i)
        {
            if (rects[i].w == 0 || rects[i].h == 0)
            {
                rects[i].x = rects[i].y = 0;  // empty rect needs no space
            }
            else
            {
                stbrp__findresult fr = stbrp__skyline_pack_rectangle(ref context, rects[i].w, rects[i].h);
                if (fr.prev_link_index != -1)
                {
                    rects[i].x = (stbrp_coord)fr.x;
                    rects[i].y = (stbrp_coord)fr.y;
                }
                else
                {
                    rects[i].x = rects[i].y = STBRP__MAXVAL;
                }
            }
        }

        // unsort
        Array.Sort(rects, (a, b) => rect_original_order(ref a, ref b));

        // set was_packed flags and all_rects_packed status
        for (i = 0; i < num_rects; ++i)
        {
            rects[i].was_packed = (rects[i].x == STBRP__MAXVAL && rects[i].y == STBRP__MAXVAL) ? 0 : 1;

            if (rects[i].was_packed == 0)
                all_rects_packed = 0;
        }

        // return the all_rects_packed status
        return all_rects_packed;
    }

    public struct stbrp_rect
    {
        // reserved for your use:
        public int id;

        // input:
        public stbrp_coord w, h;

        // output:
        public stbrp_coord x, y;
        public int was_packed;  // non-zero if valid packing

    }; // 16 bytes, nominally

    // Initialize a rectangle packer to:
    //    pack a rectangle that is 'width' by 'height' in dimensions
    //    using temporary storage provided by the array 'nodes', which is 'num_nodes' long
    //
    // You must call this function every time you start packing into a new target.
    //
    // There is no "shutdown" function. The 'nodes' memory must stay valid for
    // the following stbrp_pack_rects() call (or calls), but can be freed after
    // the call (or calls) finish.
    //
    // Note: to guarantee best results, either:
    //       1. make sure 'num_nodes' >= 'width'
    //   or  2. call stbrp_allow_out_of_mem() defined below with 'allow_out_of_mem = 1'
    //
    // If you don't do either of the above things, widths will be quantized to multiples
    // of small integers to guarantee the algorithm doesn't run out of temporary storage.
    //
    // If you do #2, then the non-quantized algorithm will be used, but the algorithm
    // may run out of temporary storage and be unable to pack some rectangles.
    static public void stbrp_init_target(out stbrp_context context, int width, int height, stbrp_node[] nodes, int num_nodes)
    {
        int i;

        for (i = 0; i < num_nodes; ++i)
        {
            nodes[i].index = i;
            nodes[i].next_index = i + 1;
        }

        nodes[num_nodes - 1].next_index = -1;

        context = new stbrp_context();

        context.init_mode = STBRP__INIT.skyline;
        context.heuristic = STBRP_HEURISTIC.Skyline_default;
        context.free_head_index = nodes[0].index;

        context.active_head_index = num_nodes;
        context.width = width;
        context.height = height;
        context.nodes = nodes;
        context.num_nodes = num_nodes;

        stbrp_setup_allow_out_of_mem(ref context, false);

        // node 0 is the full width, node 1 is the sentinel (lets us not store width explicitly)
        context.extra[0].index = num_nodes;
        context.extra[1].index = num_nodes + 1;

        context.extra[0].x = 0;
        context.extra[0].y = 0;
        context.extra[0].next_index = context.extra[1].index;
        context.extra[1].x = (stbrp_coord)width;
        context.extra[1].y = (1 << 30);
        context.extra[1].next_index = -1;
    }

    static public ref stbrp_node GetNode(ref stbrp_context context, int index)
    {
        if (index >= context.num_nodes)
        {
            return ref context.extra[index - context.num_nodes];
        }
        else
        {
            return ref context.nodes[index - context.num_nodes];
        }
    }

    static public ref stbrp_node GetNextNode(ref stbrp_context context, int index)
    {
        ref var node = ref GetNode(ref context, index);

        return ref GetNode(ref context, node.next_index);
    }


    // Optionally call this function after init but before doing any packing to
    // change the handling of the out-of-temp-memory scenario, described above.
    // If you call init again, this will be reset to the default (false).
    static public void stbrp_setup_allow_out_of_mem(ref stbrp_context context, bool allow_out_of_mem)
    {
        if (allow_out_of_mem)
        {
            // if it's ok to run out of memory, then don't bother aligning them;
            // this gives better packing, but may fail due to OOM (even though
            // the rectangles easily fit). @TODO a smarter approach would be to only
            // quantize once we've hit OOM, then we could get rid of this parameter.
            context.align = 1;
        }
        else
        {
            // if it's not ok to run out of memory, then quantize the widths
            // so that num_nodes is always enough nodes.
            //
            // I.e. num_nodes * align >= width
            //                  align >= width / num_nodes
            //                  align = ceil(width/num_nodes)

            context.align = (context.width + context.num_nodes - 1) / context.num_nodes;
        }
    }


    // Optionally select which packing heuristic the library should use. Different
    // heuristics will produce better/worse results for different data sets.
    // If you call init again, this will be reset to the default.
    static public void stbrp_setup_heuristic(ref stbrp_context context, STBRP_HEURISTIC heuristic)
    {
        switch (context.init_mode)
        {
            case STBRP__INIT.skyline:
                STBRP_ASSERT(heuristic == STBRP_HEURISTIC.Skyline_BL_sortHeight || heuristic == STBRP_HEURISTIC.Skyline_BF_sortHeight);
                context.heuristic = heuristic;
                break;

            default:
                STBRP_ASSERT(false);
                break;
        }
    }

    public enum STBRP_HEURISTIC
    {
        Skyline_default = 0,
        Skyline_BL_sortHeight = Skyline_default,
        Skyline_BF_sortHeight
    };


    //////////////////////////////////////////////////////////////////////////////
    //
    // the details of the following structures don't matter to you, but they must
    // be visible so you can handle the memory allocations for them

    public struct stbrp_node
    {
        public stbrp_coord x, y;
        public int index;
        public int next_index;
    };

    public struct stbrp_context
    {
        public int width;
        public int height;
        public int align;
        public STBRP__INIT init_mode;
        public STBRP_HEURISTIC heuristic;
        public int num_nodes;
        public int active_head_index = -1;
        public int free_head_index = -1;
        public stbrp_node[] nodes;
        public stbrp_node[] extra = new stbrp_node[2]; // we allocate two extra nodes so optimal user-node-count is 'width' not 'width+2'

        public stbrp_context()
        {
        }
    };


    //////////////////////////////////////////////////////////////////////////////
    //
    //     IMPLEMENTATION SECTION
    //

    [Conditional("DEBUG")]
    static private void STBRP_ASSERT([DoesNotReturnIf(false)] bool condition, [CallerArgumentExpression(nameof(condition))] string? message = null)
    {
        Debug.Assert(condition, message, string.Empty);
    }


    public enum STBRP__INIT
    {
        skyline = 1
    };



    // find minimum y position if it starts at x1
    static int stbrp__skyline_find_min_y(ref stbrp_context c, int first_index, int x0, int width, out int pwaste)
    {
        ref stbrp_node node = ref GetNode(ref c, first_index);

        int x1 = x0 + width;
        int min_y, visited_width, waste_area;

        //STBRP__NOTUSED(c);

        STBRP_ASSERT(GetNode(ref c, first_index).x <= x0);

#if FALSE
   // skip in case we're past the node
   while (node.next.x <= x0)
      ++node;
#else
        STBRP_ASSERT(GetNode(ref c, node.next_index).x > x0); // we ended up handling this in the caller for efficiency
#endif

        STBRP_ASSERT(node.x <= x0);

        min_y = 0;
        waste_area = 0;
        visited_width = 0;
        while (node.x < x1)
        {
            if (node.y > min_y)
            {
                // raise min_y higher.
                // we've accounted for all waste up to min_y,
                // but we'll now add more waste for everything we've visted
                waste_area += visited_width * (node.y - min_y);
                min_y = node.y;
                // the first time through, visited_width might be reduced
                if (node.x < x0)
                    visited_width += GetNode(ref c, node.next_index).x - x0;
                else
                    visited_width += GetNode(ref c, node.next_index).x - node.x;
            }
            else
            {
                // add waste area
                int under_width = GetNode(ref c, node.next_index).x - node.x;
                if (under_width + visited_width > width)
                    under_width = width - visited_width;
                waste_area += under_width * (min_y - node.y);
                visited_width += under_width;
            }
            node = ref GetNode(ref c, node.next_index);
        }

        pwaste = waste_area;
        return min_y;
    }

    struct stbrp__findresult
    {
        public int x, y;
        public int prev_link_index;
    }

    static stbrp__findresult stbrp__skyline_find_best_pos(ref stbrp_context c, int width, int height)
    {
        int best_waste = (1 << 30), best_x, best_y = (1 << 30);
        stbrp__findresult fr = new();

        int prev = -1, node = -1, tail = -1, best = -1;

        // align to multiple of c.align
        width = (width + c.align - 1);
        width -= width % c.align;
        STBRP_ASSERT(width % c.align == 0);

        // if it can't possibly fit, bail immediately
        if (width > c.width || height > c.height)
        {
            fr.prev_link_index = -1;
            fr.x = fr.y = 0;
            return fr;
        }

        node = c.active_head_index;
        prev = c.active_head_index;
        while (GetNode(ref c, node).x + width <= c.width)
        {
            int y, waste;
            y = stbrp__skyline_find_min_y(ref c, node, GetNode(ref c, node).x, width, out waste);
            if (c.heuristic == STBRP_HEURISTIC.Skyline_BL_sortHeight)
            { // actually just want to test BL
              // bottom left
                if (y < best_y)
                {
                    best_y = y;
                    best = prev;
                }
            }
            else
            {
                // best-fit
                if (y + height <= c.height)
                {
                    // can only use it if it first vertically
                    if (y < best_y || (y == best_y && waste < best_waste))
                    {
                        best_y = y;
                        best_waste = waste;
                        best = prev;
                    }
                }
            }
            prev = GetNode(ref c, node).next_index;
            node = GetNode(ref c, node).next_index;
        }

        best_x = (best == -1) ? 0 : GetNode(ref c, best).x;

        // if doing best-fit (BF), we also have to try aligning right edge to each node position
        //
        // e.g, if fitting
        //
        //     ____________________
        //    |____________________|
        //
        //            into
        //
        //   |                         |
        //   |             ____________|
        //   |____________|
        //
        // then right-aligned reduces waste, but bottom-left BL is always chooses left-aligned
        //
        // This makes BF take about 2x the time

        if (c.heuristic == STBRP_HEURISTIC.Skyline_BF_sortHeight)
        {
            tail = c.active_head_index;
            node = c.active_head_index;
            prev = c.active_head_index;
            // find first node that's admissible
            while (GetNode(ref c, tail).x < width)
                tail = GetNode(ref c, tail).next_index;
            while (tail >= 0)
            {
                int xpos = GetNode(ref c, tail).x - width;
                int y, waste;
                STBRP_ASSERT(xpos >= 0);
                // find the left position that matches this
                while (GetNextNode(ref c, node).x <= xpos)
                {
                    prev = GetNode(ref c, node).next_index;
                    node = GetNode(ref c, node).next_index;
                }
                STBRP_ASSERT(GetNextNode(ref c, node).x > xpos && GetNode(ref c, node).x <= xpos);
                y = stbrp__skyline_find_min_y(ref c, node, xpos, width, out waste);
                if (y + height <= c.height)
                {
                    if (y <= best_y)
                    {
                        if (y < best_y || waste < best_waste || (waste == best_waste && xpos < best_x))
                        {
                            best_x = xpos;
                            STBRP_ASSERT(y <= best_y);
                            best_y = y;
                            best_waste = waste;
                            best = prev;
                        }
                    }
                }
                tail = GetNode(ref c, tail).next_index;
            }
        }

        fr.prev_link_index = best;
        fr.x = best_x;
        fr.y = best_y;
        return fr;
    }

    static stbrp__findresult stbrp__skyline_pack_rectangle(ref stbrp_context context, int width, int height)
    {
        // find best position according to heuristic
        stbrp__findresult res = stbrp__skyline_find_best_pos(ref context, width, height);
        int node = -1, cur = -1;

        // bail if:
        //    1. it failed
        //    2. the best node doesn't fit (we don't always check this)
        //    3. we're out of memory
        if (res.prev_link_index == -1 || res.y + height > context.height || context.free_head_index == -1)
        {
            res.prev_link_index = -1;
            return res;
        }

        // on success, create new node
        node = context.free_head_index;
        GetNode(ref context, node).x = (stbrp_coord)res.x;
        GetNode(ref context, node).y = (stbrp_coord)(res.y + height);

        context.free_head_index = GetNode(ref context, node).next_index;

        // insert the new node into the right starting point, and
        // let 'cur' point to the remaining nodes needing to be
        // stiched back in

        cur = res.prev_link_index;
        if (GetNode(ref context, cur).x < res.x)
        {
            // preserve the existing one, so start testing with the next one
            int next = GetNode(ref context, cur).next_index;
            GetNode(ref context, cur).next_index = node;
            cur = next;
        }
        else
        {
            res.prev_link_index = node;
        }

        // from here, traverse cur and free the nodes, until we get to one
        // that shouldn't be freed
        while (GetNode(ref context, cur).next_index >= 0 &&
               GetNextNode(ref context, cur).x <= res.x + width)
        {
            int next = GetNode(ref context, cur).next_index;
            // move the current node to the free list
            GetNode(ref context, cur).next_index = context.free_head_index;
            context.free_head_index = cur;
            cur = next;
        }

        // stitch the list back in
        GetNode(ref context, node).next_index = cur;

        if (GetNode(ref context, cur).x < res.x + width)
            GetNode(ref context, cur).x = (stbrp_coord)(res.x + width);

# if _DEBUG
        cur = context.active_head;
        while (cur.x < context.width)
        {
            STBRP_ASSERT(cur.x < cur.next.x);
            cur = cur.next;
        }
        STBRP_ASSERT(cur.next == NULL);

        {
            int count = 0;
            cur = context.active_head;
            while (cur)
            {
                cur = cur.next;
                ++count;
            }
            cur = context.free_head;
            while (cur)
            {
                cur = cur.next;
                ++count;
            }
            STBRP_ASSERT(count == context.num_nodes + 2);
        }
#endif

        return res;
    }

    static private int rect_height_compare(ref stbrp_rect p, ref stbrp_rect q)
    {
        if (p.h > q.h)
            return -1;
        if (p.h < q.h)
            return 1;
        return (p.w > q.w) ? -1 : (p.w < q.w) ? 1 : 0;
    }

    static private int rect_original_order(ref stbrp_rect p, ref stbrp_rect q)
    {
        return (p.was_packed < q.was_packed) ? -1 : (p.was_packed > q.was_packed) ? 1 : 0;
    }

    /*
    ------------------------------------------------------------------------------
    This software is available under 2 licenses -- choose whichever you prefer.
    ------------------------------------------------------------------------------
    ALTERNATIVE A - MIT License
    Copyright (c) 2017 Sean Barrett
    Permission is hereby granted, free of charge, to any person obtaining a copy of
    this software and associated documentation files (the "Software"), to deal in
    the Software without restriction, including without limitation the rights to
    use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
    of the Software, and to permit persons to whom the Software is furnished to do
    so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
    ------------------------------------------------------------------------------
    ALTERNATIVE B - Public Domain (www.unlicense.org)
    This is free and unencumbered software released into the public domain.
    Anyone is free to copy, modify, publish, use, compile, sell, or distribute this
    software, either in source code form or as a compiled binary, for any purpose,
    commercial or non-commercial, and by any means.
    In jurisdictions that recognize copyright laws, the author or authors of this
    software dedicate any and all copyright interest in the software to the public
    domain. We make this dedication for the benefit of the public at large and to
    the detriment of our heirs and successors. We intend this dedication to be an
    overt act of relinquishment in perpetuity of all present and future rights to
    this software under copyright law.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
    ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    ------------------------------------------------------------------------------
    */
}