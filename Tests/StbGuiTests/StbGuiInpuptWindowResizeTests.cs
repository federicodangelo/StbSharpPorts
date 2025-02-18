using System.Net;

namespace StbSharp.Tests;

public class StbGuiInpuptWindowResizeTests : StbGuiTestsBase
{
    const float window_top = 15;
    const float window_left = 15;
    const float window_width = 20;
    const float window_height = 16;
    const float window_right = window_left + window_width;
    const float window_bottom = window_top + window_height;


    [Theory, CombinatorialData]
    public void TestWindowResize(
        [CombinatorialValues(-1, 0, 1)] int direction_x,
        [CombinatorialValues(-1, 0, 1)] int direction_y,
        [CombinatorialValues(-2, 0, 2)] float dx,
        [CombinatorialValues(-2, 0, 2)] float dy)
    {
        int window_id = RunResizeTest(direction_x, direction_y, dx, dy);

        // Assert final bounds
        var bounds = StbGui.stbg_get_widget_by_id(window_id).properties.computed_bounds.global_rect;

        if (direction_x < 0)
        {
            Assert.Equal(window_left + dx, bounds.x0);
            Assert.Equal(window_right, bounds.x1);
        }
        else if (direction_x > 0)
        {
            Assert.Equal(window_left, bounds.x0);
            Assert.Equal(window_right + dx, bounds.x1);
        }
        else
        {
            Assert.Equal(window_left, bounds.x0);
            Assert.Equal(window_right, bounds.x1);
        }

        if (direction_y < 0)
        {
            Assert.Equal(window_top + dy, bounds.y0);
            Assert.Equal(window_bottom, bounds.y1);
        }
        else if (direction_y > 0)
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

    [Theory, CombinatorialData]
    public void TestWindowWithButtonInsideResize(
        [CombinatorialValues(-1, 0, 1)] int direction_x,
        [CombinatorialValues(-1, 0, 1)] int direction_y,
        [CombinatorialValues(-20, 0, 20)] float dx,
        [CombinatorialValues(-20, 0, 20)] float dy)
    {
        // This is the minimum size of the window with a button inside:
        // /------------\
        // |Window 1    |
        // \------------/
        // |            |
        // |            |
        // | /--------\ |
        // | |        | |
        // | | Button | |
        // | |        | |
        // | \--------/ |
        // |            |
        // \------------/
        float min_window_width = 14;
        float min_window_height = 12;

        int window_id = RunResizeTest(direction_x, direction_y, dx, dy, () => StbGui.stbg_button("Button"));

        // Assert final bounds always fits the button inside
        var bounds = StbGui.stbg_get_widget_by_id(window_id).properties.computed_bounds.global_rect;

        if (direction_x < 0)
        {
            Assert.True((bounds.x1 - bounds.x0) >= min_window_width);
            Assert.Equal(window_right, bounds.x1);
        }
        else if (direction_x > 0)
        {
            Assert.Equal(window_left, bounds.x0);
            Assert.True((bounds.x1 - bounds.x0) >= min_window_width);
        }
        else
        {
            Assert.Equal(window_left, bounds.x0);
            Assert.Equal(window_right, bounds.x1);
        }

        if (direction_y < 0)
        {
            Assert.True((bounds.y1 - bounds.y0) >= min_window_height);
            Assert.Equal(window_bottom, bounds.y1);
        }
        else if (direction_y > 0)
        {
            Assert.Equal(window_top, bounds.y0);
            Assert.True((bounds.y1 - bounds.y0) >= min_window_height);
        }
        else
        {
            Assert.Equal(window_top, bounds.y0);
            Assert.Equal(window_bottom, bounds.y1);
        }
    }

    [Theory, CombinatorialData]
    public void TestWindowInsideContainerIsNotResizable(
        [CombinatorialValues(-1, 0, 1)] int direction_x,
        [CombinatorialValues(-1, 0, 1)] int direction_y,
        [CombinatorialValues(-20, 0, 20)] float dx,
        [CombinatorialValues(-20, 0, 20)] float dy)
    {
        int window_id = RunResizeTest(direction_x, direction_y, dx, dy,
            null,
            () =>
            {
                StbGui.stbg_begin_container("MyContainer", StbGui.STBG_CHILDREN_LAYOUT_DIRECTION.HORIZONTAL);
                StbGui.stbg_set_widget_position(StbGui.stbg_get_last_widget_id(), window_left, window_top);
            },
            () =>
            {
                StbGui.stbg_end_container();
            }
        );

        // Assert final bounds always fits the button inside
        var bounds = StbGui.stbg_get_widget_by_id(window_id).properties.computed_bounds.global_rect;

        Assert.Equal(window_left, bounds.x0);
        Assert.Equal(window_right, bounds.x1);
        Assert.Equal(window_top, bounds.y0);
        Assert.Equal(window_bottom, bounds.y1);
    }

    private static int RunResizeTest(int direction_x, int direction_y, float dx, float dy, Action? create_other_children = null, Action? create_parent = null, Action? end_parent = null)
    {
        create_other_children ??= () => { };
        create_parent ??= () => { };
        end_parent ??= () => { };

        InitGUI();

        StbGui.stbg_rect bounds;
        int window_id;

        // Initial setup
        StbGui.stbg_begin_frame();
        {
            create_parent();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    window_id = StbGui.stbg_get_last_widget_id();
                    StbGui.stbg_set_widget_position(window_id, window_top, window_left);
                    StbGui.stbg_set_widget_size(window_id, window_width, window_height);

                    create_other_children();

                }
                StbGui.stbg_end_window();
            }
            end_parent();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        bounds = StbGui.stbg_get_widget_by_id(window_id).properties.computed_bounds.global_rect;

        Assert.Equal(window_left, bounds.x0);
        Assert.Equal(window_right, bounds.x1);
        Assert.Equal(window_top, bounds.y0);
        Assert.Equal(window_bottom, bounds.y1);

        float start_mouse_position_x;
        float start_mouse_position_y;

        if (direction_x < 0)
            start_mouse_position_x = window_left;
        else if (direction_x > 0)
            start_mouse_position_x = window_right;
        else
            start_mouse_position_x = window_left + (window_right - window_left) / 2;

        if (direction_y < 0)
            start_mouse_position_y = window_top;
        else if (direction_y > 0)
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
            create_parent();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    window_id = StbGui.stbg_get_last_widget_id();

                    create_other_children();
                }
                StbGui.stbg_end_window();
            }
            end_parent();
        }
        StbGui.stbg_end_frame();

        if ((direction_x != 0 || direction_y != 0) && StbGui.stbg_get_widget_by_id(StbGui.stbg_get_widget_by_id(window_id).hierarchy.parent_id).properties.layout.children_layout_direction == StbGui.STBG_CHILDREN_LAYOUT_DIRECTION.FREE)
        {
            // Cursor should NOT be the default (it should be one of the RESIZE_* ones)
            Assert.NotEqual(StbGui.STBG_ACTIVE_CURSOR_TYPE.DEFAULT, StbGui.stbg_get_cursor());
        }
        else
        {
            Assert.Equal(StbGui.STBG_ACTIVE_CURSOR_TYPE.DEFAULT, StbGui.stbg_get_cursor());
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
            create_parent();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    window_id = StbGui.stbg_get_last_widget_id();

                    create_other_children();
                }
                StbGui.stbg_end_window();
            }
            end_parent();
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
            create_parent();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    window_id = StbGui.stbg_get_last_widget_id();

                    create_other_children();
                }
                StbGui.stbg_end_window();
            }
            end_parent();
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
            create_parent();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    window_id = StbGui.stbg_get_last_widget_id();

                    create_other_children();
                }
                StbGui.stbg_end_window();
            }
            end_parent();
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
            create_parent();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    window_id = StbGui.stbg_get_last_widget_id();

                    create_other_children();
                }
                StbGui.stbg_end_window();
            }
            end_parent();
        }
        StbGui.stbg_end_frame();

        return window_id;
    }
}
