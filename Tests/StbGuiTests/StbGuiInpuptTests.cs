namespace StbSharp.Tests;

public class StbGuiInputTests : StbGuiTestsBase
{
    [Fact]
    public void TestButtonHover()
    {
        InitGUI();

        // Normal render
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "CB12"],
            ["""|          |""", "CB1CT10CB1"],
            ["""| Button 1 |""", "CB1CT1CW8CT1CB1"],
            ["""|          |""", "CB1CT10CB1"],
            ["""\----------/""", "CB12"],
        ]);

        // Hover
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(2, 2),
            mouse_position_valid = true
        });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "BW12"],
            ["""|          |""", "BW1BT10BW1"],
            ["""| Button 1 |""", "BW1BT1BK8BT1BW1"],
            ["""|          |""", "BW1BT10BW1"],
            ["""\----------/""", "BW12"],
        ]);
    }

    [Fact]
    public void TestButtonHoverOff()
    {
        InitGUI();

        // Normal render
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "CB12"],
            ["""|          |""", "CB1CT10CB1"],
            ["""| Button 1 |""", "CB1CT1CW8CT1CB1"],
            ["""|          |""", "CB1CT10CB1"],
            ["""\----------/""", "CB12"],
        ]);

        // Hover
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(2, 2),
            mouse_position_valid = true
        });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "BW12"],
            ["""|          |""", "BW1BT10BW1"],
            ["""| Button 1 |""", "BW1BT1BK8BT1BW1"],
            ["""|          |""", "BW1BT10BW1"],
            ["""\----------/""", "BW12"],
        ]);

        // Hover off
        StbGui.stbg_set_input(new()
        {
            mouse_position_valid = false
        });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "CB12"],
            ["""|          |""", "CB1CT10CB1"],
            ["""| Button 1 |""", "CB1CT1CW8CT1CB1"],
            ["""|          |""", "CB1CT10CB1"],
            ["""\----------/""", "CB12"],
        ]);
    }

    [Fact]
    public void TestButtonPressed()
    {
        InitGUI();

        // Normal render
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "CB12"],
            ["""|          |""", "CB1CT10CB1"],
            ["""| Button 1 |""", "CB1CT1CW8CT1CB1"],
            ["""|          |""", "CB1CT10CB1"],
            ["""\----------/""", "CB12"],
        ]);

        // Press
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(2, 2),
            mouse_position_valid = true,
            mouse_button_1_down = true,
        });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "GW12"],
            ["""|          |""", "GW1GT10GW1"],
            ["""| Button 1 |""", "GW1GT1GK8GT1GW1"],
            ["""|          |""", "GW1GT10GW1"],
            ["""\----------/""", "GW12"],
        ]);
    }

    [Fact]
    public void TestButtonClick()
    {
        InitGUI();

        // Normal render
        StbGui.stbg_begin_frame();
        {
            Assert.False(StbGui.stbg_button("Button 1"));
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "CB12"],
            ["""|          |""", "CB1CT10CB1"],
            ["""| Button 1 |""", "CB1CT1CW8CT1CB1"],
            ["""|          |""", "CB1CT10CB1"],
            ["""\----------/""", "CB12"],
        ]);

        // Press
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(2, 2),
            mouse_position_valid = true,
            mouse_button_1_down = true,
            mouse_button_1_pressed = true,
        });

        StbGui.stbg_begin_frame();
        {
            Assert.False(StbGui.stbg_button("Button 1"));
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "GW12"],
            ["""|          |""", "GW1GT10GW1"],
            ["""| Button 1 |""", "GW1GT1GK8GT1GW1"],
            ["""|          |""", "GW1GT10GW1"],
            ["""\----------/""", "GW12"],
        ]);

        // Release (triggers click)
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(2, 2),
            mouse_position_valid = true,
            mouse_button_1_released = true,
            mouse_button_1_down = false,
        });

        StbGui.stbg_begin_frame();
        {
            Assert.True(StbGui.stbg_button("Button 1"));
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "BW12"],
            ["""|          |""", "BW1BT10BW1"],
            ["""| Button 1 |""", "BW1BT1BK8BT1BW1"],
            ["""|          |""", "BW1BT10BW1"],
            ["""\----------/""", "BW12"],
        ]);


        // Frame after click (should not triggers click anymore)
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(2, 2),
            mouse_position_valid = true,
            mouse_button_1_down = false,
        });

        StbGui.stbg_begin_frame();
        {
            Assert.False(StbGui.stbg_button("Button 1"));
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------\""", "BW12"],
            ["""|          |""", "BW1BT10BW1"],
            ["""| Button 1 |""", "BW1BT1BK8BT1BW1"],
            ["""|          |""", "BW1BT10BW1"],
            ["""\----------/""", "BW12"],
        ]);
    }

    [Fact]
    public void TestWindowHover()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 11);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 18);

        // Normal render
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {

            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------\""", "MW18"],
            ["""|Window 1        |""", "MW9MT8MW1"],
            ["""\----------------/""", "MW18"],
            ["""|                |""", "BW1BT16BW1"],
            ["""|                |""", "BW1BT16BW1"],
            ["""|                |""", "BW1BT16BW1"],
            ["""|                |""", "BW1BT16BW1"],
            ["""|                |""", "BW1BT16BW1"],
            ["""|                |""", "BW1BT16BW1"],
            ["""|                |""", "BW1BT16BW1"],
            ["""\----------------/""", "BW18"],
        ]);

        // Hover middle of window
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(8, 10),
            mouse_position_valid = true,
            mouse_button_1_down = false,
        });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {

            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------\""", "WW18"],
            ["""|Window 1        |""", "WW1WM8WT8WW1"],
            ["""\----------------/""", "WW18"],
            ["""|                |""", "BW1BT16BW1"],
            ["""|                |""", "BW1BT16BW1"],
            ["""|                |""", "BW1BT16BW1"],
            ["""|                |""", "BW1BT16BW1"],
            ["""|                |""", "BW1BT16BW1"],
            ["""|                |""", "BW1BT16BW1"],
            ["""|                |""", "BW1BT16BW1"],
            ["""\----------------/""", "BW18"],
        ]);
    }

    [Fact]
    public void TestWindowClickBringsToFront()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 11);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 18);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_SPACING_BETWEEN_NEW_WINDOWS, 4);

        // Normal render
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {

            }
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            {

            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------\    """, "MW18CT4"],
            ["""|Window 1        |    """, "MW9MT8MW1CT4"],
            ["""\----------------/    """, "MW18CT4"],
            ["""|                |    """, "BW1BT16BW1CT4"],
            ["""|   /----------------\""", "BW1BT3MW18"],
            ["""|   |Window 2        |""", "BW1BT3MW9MT4MW1MT3MW1"],
            ["""|   \----------------/""", "BW1BT3MW18"],
            ["""|   |                |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""|   |                |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""|   |                |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""\---|                |""", "BW18BT3BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    \----------------/""", "CT4BW18"],
        ]);

        // Hover near Window 1 and press mouse button, Window 1 should be on top
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(2, 2),
            mouse_position_valid = true,
            mouse_button_1_down = true,
        });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {

            }
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            {

            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------\    """, "WW18CT4"],
            ["""|Window 1        |    """, "WW1WM8WT8WW1CT4"],
            ["""\----------------/    """, "WW18CT4"],
            ["""|                |    """, "BW1BT16BW1CT4"],
            ["""|                |---\""", "BW1BT3BW14MW4"],
            ["""|                |   |""", "BW1BT3BW9BT4BW1MT3MW1"],
            ["""|                |---/""", "BW1BT3BW14MW4"],
            ["""|                |   |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""|                |   |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""|                |   |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""\----------------/   |""", "BW18BT3BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    \----------------/""", "CT4BW18"],
        ]);

        // Hover near Window 1 and release mouse button, Window 1 should still be on top
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(2, 2),
            mouse_position_valid = true,
            mouse_button_1_down = false,
        });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {

            }
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            {

            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------\    """, "WW18CT4"],
            ["""|Window 1        |    """, "WW1WM8WT8WW1CT4"],
            ["""\----------------/    """, "WW18CT4"],
            ["""|                |    """, "BW1BT16BW1CT4"],
            ["""|                |---\""", "BW1BT3BW14MW4"],
            ["""|                |   |""", "BW1BT3BW9BT4BW1MT3MW1"],
            ["""|                |---/""", "BW1BT3BW14MW4"],
            ["""|                |   |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""|                |   |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""|                |   |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""\----------------/   |""", "BW18BT3BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    \----------------/""", "CT4BW18"],
        ]);
    }

    [Fact]
    public void TestWindowDragging()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 11);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 18);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_SPACING_BETWEEN_NEW_WINDOWS, 4);

        // Normal render
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {

            }
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            {

            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------\    """, "MW18CT4"],
            ["""|Window 1        |    """, "MW9MT8MW1CT4"],
            ["""\----------------/    """, "MW18CT4"],
            ["""|                |    """, "BW1BT16BW1CT4"],
            ["""|   /----------------\""", "BW1BT3MW18"],
            ["""|   |Window 2        |""", "BW1BT3MW9MT4MW1MT3MW1"],
            ["""|   \----------------/""", "BW1BT3MW18"],
            ["""|   |                |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""|   |                |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""|   |                |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""\---|                |""", "BW18BT3BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    \----------------/""", "CT4BW18"],
        ]);

        // Hover near Window 1 and press mouse button, Window 1 should be on top
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(2, 2),
            mouse_position_valid = true,
            mouse_button_1_down = true,
            mouse_button_1_pressed = true,
        });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {

            }
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            {

            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------\    """, "WW18CT4"],
            ["""|Window 1        |    """, "WW1WM8WT8WW1CT4"],
            ["""\----------------/    """, "WW18CT4"],
            ["""|                |    """, "BW1BT16BW1CT4"],
            ["""|                |---\""", "BW1BT3BW14MW4"],
            ["""|                |   |""", "BW1BT3BW9BT4BW1MT3MW1"],
            ["""|                |---/""", "BW1BT3BW14MW4"],
            ["""|                |   |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""|                |   |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""|                |   |""", "BW1BT3BW1BT12BW1BT3BW1"],
            ["""\----------------/   |""", "BW18BT3BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    \----------------/""", "CT4BW18"],
        ]);

        // Hover near Window 1 and drag mouse, window should be moved
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(4, 2),
            mouse_position_valid = true,
            mouse_button_1_pressed = true,
        });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {

            }
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            {

            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""  /----------------\  """, "CT2WW18CT2"],
            ["""  |Window 1        |  """, "CT2WW1WM8WT8WW1CT2"],
            ["""  \----------------/  """, "CT2WW18CT2"],
            ["""  |                |  """, "CT2BW1BT16BW1CT2"],
            ["""  |                |-\""", "CT2BW1BT1BW16MW2"],
            ["""  |                | |""", "CT2BW1BT1BW9BT6BW1MT1MW1"],
            ["""  |                |-/""", "CT2BW1BT1BW16MW2"],
            ["""  |                | |""", "CT2BW1BT1BW1BT14BW1BT1BW1"],
            ["""  |                | |""", "CT2BW1BT1BW1BT14BW1BT1BW1"],
            ["""  |                | |""", "CT2BW1BT1BW1BT14BW1BT1BW1"],
            ["""  \----------------/ |""", "CT2BW18BT1BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    \----------------/""", "CT4BW18"],
        ]);

        // Release mouse button but keep moving cursor, window should be at the last position
        StbGui.stbg_set_input(new()
        {
            mouse_position = StbGui.stbg_build_position(6, 2),
            mouse_position_valid = true,
            mouse_button_1_released = true,
            mouse_button_1_down = false,
        });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {

            }
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            {

            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""  /----------------\  """, "CT2WW18CT2"],
            ["""  |Window 1        |  """, "CT2WW1WM8WT8WW1CT2"],
            ["""  \----------------/  """, "CT2WW18CT2"],
            ["""  |                |  """, "CT2BW1BT16BW1CT2"],
            ["""  |                |-\""", "CT2BW1BT1BW16MW2"],
            ["""  |                | |""", "CT2BW1BT1BW9BT6BW1MT1MW1"],
            ["""  |                |-/""", "CT2BW1BT1BW16MW2"],
            ["""  |                | |""", "CT2BW1BT1BW1BT14BW1BT1BW1"],
            ["""  |                | |""", "CT2BW1BT1BW1BT14BW1BT1BW1"],
            ["""  |                | |""", "CT2BW1BT1BW1BT14BW1BT1BW1"],
            ["""  \----------------/ |""", "CT2BW18BT1BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    |                |""", "CT4BW1BT16BW1"],
            ["""    \----------------/""", "CT4BW18"],
        ]);
    }
}
