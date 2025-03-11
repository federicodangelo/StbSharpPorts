namespace StbSharp.Examples;

static public class DebugWindow
{
    static public void Render(StbGuiAppBase appBase, StbGuiStringMemoryPool mp)
    {
        var metrics = appBase.Metrics;
        StbGui.stbg_label(mp.Build("FPS: ") + appBase.Metrics.Fps + " Skipped Frames: " + metrics.SkippedFrames + " [" + appBase.RenderBackend + "]");
        StbGui.stbg_label(mp.Build("Allocated Bytes: ") + metrics.LastSecondAllocatedBytes + " Per Frame: " + (metrics.LastSecondAllocatedBytes / (metrics.Fps > 0 ? metrics.Fps : 1)) + " GC: " + metrics.TotalGarbageCollectionsPerformed);
        StbGui.stbg_label(mp.Build("MP Used Characters: ") + StbGui.stbg_get_frame_stats().string_memory_pool_used_characters + " Overflown: " + StbGui.stbg_get_frame_stats().string_memory_pool_overflowed_characters);
        StbGui.stbg_label(mp.Build("Process input time : ").Append(metrics.average_performance_metrics.process_input_time_us / 1000.0f, 3) + " ms");
        StbGui.stbg_label(mp.Build("Layout widgets time: ").Append(metrics.average_performance_metrics.layout_widgets_time_us / 1000.0f, 3) + " ms");
        StbGui.stbg_label(mp.Build("Hash time time     : ").Append(metrics.average_performance_metrics.hash_time_us / 1000.0f, 3) + " ms");
        StbGui.stbg_label(mp.Build("Render time        : ").Append(metrics.average_performance_metrics.render_time_us / 1000.0f, 3) + " ms");
        //Console.WriteLine(appBase.Metrics.LastSecondAllocatedBytes / (metrics.Fps > 0 ? metrics.Fps : 1));
    }
}
