namespace StbSharp.Tests;

public class StbGuiInpuptWindowResizeTests : StbGuiTestsBase
{
    const float window_top = 5;
    const float window_left = 5;
    const float window_width = 10;
    const float window_height = 8;
    const float window_right = window_left + window_width;
    const float window_bottom = window_top + window_height;


    [Theory, CombinatorialData]

    public void TestWindowResize(
        [CombinatorialValues(-2, 0, 2)] float dx, 
        [CombinatorialValues(-2, 0, 2)] float dy)
    {
        InitGUI();

        int window_id;
        StbGui.stbg_rect bounds;

        // Initial setup
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                window_id = StbGui.stbg_get_last_widget_id();
                StbGui.stbg_move_window(window_id, window_top, window_left);
                StbGui.stbg_resize_window(window_id, window_width, window_height);

            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        bounds = StbGui.stbg_get_widget_by_id(window_id).properties.computed_bounds.global_rect;

        Assert.Equal(window_left, bounds.x0);
        Assert.Equal(window_right, bounds.x1);
        Assert.Equal(window_top, bounds.y0);
        Assert.Equal(window_bottom, bounds.y1);

        float start_mouse_position_x;
        float start_mouse_position_y;

        if (dx < 0)
            start_mouse_position_x = window_left;
        else if (dx > 0)
            start_mouse_position_x = window_right;
        else
            start_mouse_position_x = window_left + (window_right - window_left) / 2;

        if (dy < 0)
            start_mouse_position_y = window_top;
        else if (dy > 0)
            start_mouse_position_y = window_bottom;
        else
            start_mouse_position_y = window_top + (window_bottom - window_top) / 2;

        // Hover near window and press mouse button, this mark the window as pressed
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(start_mouse_position_x, start_mouse_position_y),
            mouse_position_valid = true,
            mouse_button_1_down = true,
            mouse_button_1_pressed = true,
        });
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                window_id = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        if (dx != 0 || dy != 0)
        {
            // Cursor should NOT be the default (it should be one of the RESIZE_* ones)
            Assert.NotEqual(StbGui.STBG_ACTIVE_CURSOR_TYPE.DEFAULT, StbGui.stbg_get_cursor());
        }

        // Wait another frame so the dragging operations starts
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(start_mouse_position_x, start_mouse_position_y),
            mouse_position_valid = true,
            mouse_button_1_pressed = true,
        });
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                window_id = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        // Now drag the mouse
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(start_mouse_position_x + dx, start_mouse_position_y + dy),
            mouse_position_valid = true,
            mouse_button_1_pressed = true,
        });
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                window_id = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        // Now release the mouse
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(start_mouse_position_x + dx, start_mouse_position_y + dy),
            mouse_position_valid = true,
            mouse_button_1_released = true,
        });
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                window_id = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        // Now don't hover the window any more
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(start_mouse_position_x + dx, start_mouse_position_y + dy),
            mouse_position_valid = false,
            mouse_button_1_released = false,
            mouse_button_1_pressed = false,
        });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                window_id = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        // Assert final bounds
        bounds = StbGui.stbg_get_widget_by_id(window_id).properties.computed_bounds.global_rect;

        if (dx < 0)
        {
            Assert.Equal(window_left + dx, bounds.x0);
            Assert.Equal(window_right, bounds.x1);
        }
        else if (dx > 0)
        {
            Assert.Equal(window_left, bounds.x0);
            Assert.Equal(window_right + dx, bounds.x1);
        }
        else
        {
            Assert.Equal(window_left, bounds.x0);
            Assert.Equal(window_right, bounds.x1);
        }

        if (dy < 0)
        {
            Assert.Equal(window_top + dy, bounds.y0);
            Assert.Equal(window_bottom, bounds.y1);
        }
        else if (dy > 0)
        {
            Assert.Equal(window_top, bounds.y0);
            Assert.Equal(window_bottom + dy, bounds.y1);
        }
        else
        {
            Assert.Equal(window_top, bounds.y0);
            Assert.Equal(window_bottom, bounds.y1);
        }

    }
}
