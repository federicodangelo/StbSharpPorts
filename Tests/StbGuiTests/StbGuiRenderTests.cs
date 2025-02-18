
namespace StbSharp.Tests;

public class StbGuiRenderTests : StbGuiTestsBase
{
    [Fact]
    public void TestRenderEmpty()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        Assert.Equal(
            [
                new StbGui.stbg_render_command() { type = StbGui.STBG_RENDER_COMMAND_TYPE.BEGIN_FRAME, bounds ={ x1 = ScreenSizeWidth, y1 = ScreenSizeHeight }, background_color = StbGui.stbg_get_widget_style_color(StbGui.STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR)},
                new StbGui.stbg_render_command() { type = StbGui.STBG_RENDER_COMMAND_TYPE.END_FRAME}
            ],
            render_commands_all
        );

        RenderCommandsToTestScreen();

        Assert.Equal(test_render_screen.GetTestRenderScreenPixel(0, 0).background_color, StbGui.stbg_get_widget_style_color(StbGui.STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR));
        Assert.Equal(test_render_screen.GetTestRenderScreenPixel(ScreenSizeWidth - 1, ScreenSizeHeight - 1).background_color, StbGui.stbg_get_widget_style_color(StbGui.STBG_WIDGET_STYLE.ROOT_BACKGROUND_COLOR));
    }

    [Fact]
    public void TestRenderButton()
    {
        InitGUI();

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
    public void TestRenderButtonInDebugWindow()
    {
        InitGUI(new StbGui.stbg_init_options() { dont_nest_non_window_root_elements_into_debug_window = false });

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Button 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------------------------------\""", "RW34"],
            ["""|_DEBUG_                         |""", "RW8RT25RW1"],
            ["""\--------------------------------/""", "RW34"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""| /----------\                   |""", "BW1BT1CB12BT19BW1"],
            ["""| |          |                   |""", "BW1BT1CB1CT10CB1BT19BW1"],
            ["""| | Button 1 |                   |""", "BW1BT1CB1CT1CW8CT1CB1BT19BW1"],
            ["""| |          |                   |""", "BW1BT1CB1CT10CB1BT19BW1"],
            ["""| \----------/                   |""", "BW1BT1CB12BT19BW1"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""|                                |""", "BW1BT32BW1"],
            ["""\--------------------------------/""", "BW34"],
        ]);
    }

    [Fact]
    public void TestRenderLabel()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_label("Label 1");
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""Label 1""", "CW7"],
        ]);
    }


    [Fact]
    public void TestRenderWindow()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 11);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 18);

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
    }

    [Fact]
    public void TestRenderWindowLongTitle()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 11);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 18);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window with really really long title");
            {

            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------\""", "MW18"],
            ["""|Window with real|""", "MW18"],
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
    }

    [Fact]
    public void TestRenderWindowTwoWindows()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 14);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 18);

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

            StbGui.stbg_set_widget_position(StbGui.stbg_get_last_widget_id(), 20, 5);

        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------\                    """, "MW18CT20"],
            ["""|Window 1        |                    """, "MW9MT8MW1CT20"],
            ["""\----------------/                    """, "MW18CT20"],
            ["""|                |                    """, "BW1BT16BW1CT20"],
            ["""|                |                    """, "BW1BT16BW1CT20"],
            ["""|                |  /----------------\""", "BW1BT16BW1CT2MW18"],
            ["""|                |  |Window 2        |""", "BW1BT16BW1CT2MW9MT8MW1"],
            ["""|                |  \----------------/""", "BW1BT16BW1CT2MW18"],
            ["""|                |  |                |""", "BW1BT16BW1CT2BW1BT16BW1"],
            ["""|                |  |                |""", "BW1BT16BW1CT2BW1BT16BW1"],
            ["""|                |  |                |""", "BW1BT16BW1CT2BW1BT16BW1"],
            ["""|                |  |                |""", "BW1BT16BW1CT2BW1BT16BW1"],
            ["""|                |  |                |""", "BW1BT16BW1CT2BW1BT16BW1"],
            ["""\----------------/  |                |""", "BW18CT2BW1BT16BW1"],
            ["""                    |                |""", "CT20BW1BT16BW1"],
            ["""                    |                |""", "CT20BW1BT16BW1"],
            ["""                    |                |""", "CT20BW1BT16BW1"],
            ["""                    |                |""", "CT20BW1BT16BW1"],
            ["""                    \----------------/""", "CT20BW18"],
        ]);
    }

    [Fact]
    public void TestRenderWindowWithTwoButtons()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 17);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 24);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                StbGui.stbg_button("Button 1");
                StbGui.stbg_button("Button 2");
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------------\""", "MW24"],
            ["""|Window 1              |""", "MW9MT14MW1"],
            ["""\----------------------/""", "MW24"],
            ["""|                      |""", "BW1BT22BW1"],
            ["""|                      |""", "BW1BT22BW1"],
            ["""| /----------\         |""", "BW1BT1CB12BT9BW1"],
            ["""| |          |         |""", "BW1BT1CB1CT10CB1BT9BW1"],
            ["""| | Button 1 |         |""", "BW1BT1CB1CT1CW8CT1CB1BT9BW1"],
            ["""| |          |         |""", "BW1BT1CB1CT10CB1BT9BW1"],
            ["""| \----------/         |""", "BW1BT1CB12BT9BW1"],
            ["""| /----------\         |""", "BW1BT1CB12BT9BW1"],
            ["""| |          |         |""", "BW1BT1CB1CT10CB1BT9BW1"],
            ["""| | Button 2 |         |""", "BW1BT1CB1CT1CW8CT1CB1BT9BW1"],
            ["""| |          |         |""", "BW1BT1CB1CT10CB1BT9BW1"],
            ["""| \----------/         |""", "BW1BT1CB12BT9BW1"],
            ["""|                      |""", "BW1BT22BW1"],
            ["""\----------------------/""", "BW24"],
        ]);
    }

    [Fact]
    public void TestRenderWindowWithThreeButtons()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 17);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 24);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                StbGui.stbg_button("Button 1");
                StbGui.stbg_button("Button 2");
                StbGui.stbg_button("Button 3");
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------------\""", "MW24"],
            ["""|Window 1              |""", "MW9MT14MW1"],
            ["""\----------------------/""", "MW24"],
            ["""|                      |""", "BW1BT22BW1"],
            ["""|                      |""", "BW1BT22BW1"],
            ["""| /----------\         |""", "BW1BT1CB12BT9BW1"],
            ["""| |          |         |""", "BW1BT1CB1CT10CB1BT9BW1"],
            ["""| | Button 1 |         |""", "BW1BT1CB1CT1CW8CT1CB1BT9BW1"],
            ["""| |          |         |""", "BW1BT1CB1CT10CB1BT9BW1"],
            ["""| \----------/         |""", "BW1BT1CB12BT9BW1"],
            ["""| /----------\         |""", "BW1BT1CB12BT9BW1"],
            ["""| |          |         |""", "BW1BT1CB1CT10CB1BT9BW1"],
            ["""| | Button 2 |         |""", "BW1BT1CB1CT1CW8CT1CB1BT9BW1"],
            ["""| |          |         |""", "BW1BT1CB1CT10CB1BT9BW1"],
            ["""| \----------/         |""", "BW1BT1CB12BT9BW1"],
            ["""| /----------\         |""", "BW1BT1CB12BT9BW1"],
            ["""| |          |         |""", "BW1BT1CB1CT10CB1BT9BW1"],
            ["""| | Button 3 |         |""", "BW1BT1CB1CT1CW8CT1CB1BT9BW1"],
            ["""| |          |         |""", "BW1BT1CB1CT10CB1BT9BW1"],
            ["""| \----------/         |""", "BW1BT1CB12BT9BW1"],
            ["""|                      |""", "BW1BT22BW1"],
            ["""\----------------------/""", "BW24"],
        ]);
    }

    [Fact]
    public void TestRenderWindowWithTwoButtonsInHorizontalContainer()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 17);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 24);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                StbGui.stbg_begin_container("Container 1", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL);
                {
                    StbGui.stbg_button("Button 1");
                    StbGui.stbg_button("Button 2");
                }
                StbGui.stbg_end_container();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------------------------\""", "MW28"],
            ["""|Window 1                  |""", "MW9MT18MW1"],
            ["""\--------------------------/""", "MW28"],
            ["""|                          |""", "BW1BT26BW1"],
            ["""|                          |""", "BW1BT26BW1"],
            ["""| /----------\/----------\ |""", "BW1BT1CB24BT1BW1"],
            ["""| |          ||          | |""", "BW1BT1CB1CT10CB2CT10CB1BT1BW1"],
            ["""| | Button 1 || Button 2 | |""", "BW1BT1CB1CT1CW8CT1CB2CT1CW8CT1CB1BT1BW1"],
            ["""| |          ||          | |""", "BW1BT1CB1CT10CB2CT10CB1BT1BW1"],
            ["""| \----------/\----------/ |""", "BW1BT1CB24BT1BW1"],
            ["""|                          |""", "BW1BT26BW1"],
            ["""|                          |""", "BW1BT26BW1"],
            ["""|                          |""", "BW1BT26BW1"],
            ["""|                          |""", "BW1BT26BW1"],
            ["""|                          |""", "BW1BT26BW1"],
            ["""|                          |""", "BW1BT26BW1"],
            ["""\--------------------------/""", "BW28"],
        ]);
    }

    [Fact]
    public void TestRenderWindowWithThreeButtonsInHorizontalContainer()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 17);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 24);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                StbGui.stbg_begin_container("Container 1", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL);
                {
                    StbGui.stbg_button("Button 1");
                    StbGui.stbg_button("Button 2");
                    StbGui.stbg_button("Button 3");
                }
                StbGui.stbg_end_container();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_render();

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/--------------------------------------\""", "MW40"],
            ["""|Window 1                              |""", "MW9MT30MW1"],
            ["""\--------------------------------------/""", "MW40"],
            ["""|                                      |""", "BW1BT38BW1"],
            ["""|                                      |""", "BW1BT38BW1"],
            ["""| /----------\/----------\/----------\ |""", "BW1BT1CB36BT1BW1"],
            ["""| |          ||          ||          | |""", "BW1BT1CB1CT10CB2CT10CB2CT10CB1BT1BW1"],
            ["""| | Button 1 || Button 2 || Button 3 | |""", "BW1BT1CB1CT1CW8CT1CB2CT1CW8CT1CB2CT1CW8CT1CB1BT1BW1"],
            ["""| |          ||          ||          | |""", "BW1BT1CB1CT10CB2CT10CB2CT10CB1BT1BW1"],
            ["""| \----------/\----------/\----------/ |""", "BW1BT1CB36BT1BW1"],
            ["""|                                      |""", "BW1BT38BW1"],
            ["""|                                      |""", "BW1BT38BW1"],
            ["""|                                      |""", "BW1BT38BW1"],
            ["""|                                      |""", "BW1BT38BW1"],
            ["""|                                      |""", "BW1BT38BW1"],
            ["""|                                      |""", "BW1BT38BW1"],
            ["""\--------------------------------------/""", "BW40"],
        ]);
    }

}
