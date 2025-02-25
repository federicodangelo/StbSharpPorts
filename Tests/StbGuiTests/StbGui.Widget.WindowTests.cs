using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace StbSharp.Tests;

public class StbGuiWindowTests : StbGuiTestsBase
{
    [Fact]
    public void TestBeginAndEndWindow()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();
    }

    [Fact]
    public void TestUnbalancedBeginAndEndWindowThrowsException()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
        }
        Assert.Throws<StbGui.StbgAssertException>(() => StbGui.stbg_end_frame());
    }

    [Fact]
    public void TestDuplicatedButtonIdsInDifferentWindows()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            if (StbGui.stbg_begin_window("Window 1"))
            {
                StbGui.stbg_button("Button");
            }
            StbGui.stbg_end_window();

            if (StbGui.stbg_begin_window("Window 2"))
            {
                StbGui.stbg_button("Button");
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();
    }

    [Fact]
    public void TestWindowDisappearsInSecondFrame()
    {
        InitGUI();

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                StbGui.stbg_button("Button");
            }
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            {
                StbGui.stbg_button("Button");
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        StbGui.stbg_begin_frame();
        {
            // Skip Window 1
            StbGui.stbg_begin_window("Window 2");
            {
                StbGui.stbg_button("Button");
                StbGui.stbg_end_window();
            }
        }
        StbGui.stbg_end_frame();

        // Both Window1 and the contained button should have been destroyed
        Assert.Equal(2, StbGui.stbg_get_context().frame_stats.destroyed_widgets);
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

    [Fact]
    public void TestRenderWindowScrollableWithTenButtonsInside()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 20);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 30);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_ALLOW_SCROLLBARS, true);

        // Scrollbars take 2 frames to appear
        for (int i = 0; i < 2; i++)
        {
            StbGui.stbg_begin_frame();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    for (int b = 0; b < 10; b++)
                        StbGui.stbg_button("Button " + b);
                }
                StbGui.stbg_end_window();
            }
            StbGui.stbg_end_frame();

            StbGui.stbg_render();
        }

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------------------\""", "MW30"],
            ["""|Window 1                    |""", "MW9MT20MW1"],
            ["""\----------------------------/""", "MW30"],
            ["""|                           ^|""", "BW1BT27BW2"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""| /----------\               |""", "BW1BT1CB12BT15BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| | Button 0 |               |""", "BW1BT1CB1CT1CW8CT1CB1BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| \----------/               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| /----------\               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| | Button 1 |               |""", "BW1BT1CB1CT1CW8CT1CB1BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| \----------/               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| /----------\               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| | Button 2 |               |""", "BW1BT1CB1CT1CW8CT1CB1BT14CT1BW1"],
            ["""|                           v|""", "BW1BT27BW2"],
            ["""\----------------------------/""", "BW30"],
        ]);
    }

    [Fact]
    public void TestScrollWindowWithTenButtonsInside()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 20);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 30);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_ALLOW_SCROLLBARS, true);

        Action render = () =>
        {
            StbGui.stbg_begin_frame();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    for (int b = 0; b < 10; b++)
                        StbGui.stbg_button("Button " + b);
                }
                StbGui.stbg_end_window();
            }
            StbGui.stbg_end_frame();

            StbGui.stbg_render();

        };

        // Scrollbars take 2 frames to appear
        for (int i = 0; i < 2; i++)
            render();

        // click the "scroll down" button 3 times
        for (var times = 0; times < 3; times++)
        {
            // click the "scroll down" button
            SetMousePosition(28, 18);
            SetMouseButton1(true);

            render();

            // release the "scroll down" button
            SetMouseButton1(false);

            render();
        }

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------------------\""", "WW30"],
            ["""|Window 1                    |""", "WW1WM8WT20WW1"],
            ["""\----------------------------/""", "WW30"],
            ["""|                           ^|""", "BW1BT27BW2"],
            ["""|                            |""", "BW1BT27CT1BW1"],
            ["""| \----------/               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| /----------\               |""", "BW1BT1CB12BT15BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT15BW1"],
            ["""| | Button 2 |               |""", "BW1BT1CB1CT1CW8CT1CB1BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| \----------/               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| /----------\               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| | Button 3 |               |""", "BW1BT1CB1CT1CW8CT1CB1BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| \----------/               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| /----------\               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""|                           v|""", "BW1BT27BR1BW1"],
            ["""\----------------------------/""", "BW30"],
        ]);
    }

    [Fact]
    public void TestScrollWindowUsingMouseWheelWithTenButtonsInside()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 20);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 30);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_ALLOW_SCROLLBARS, true);

        Action render = () =>
        {
            StbGui.stbg_begin_frame();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    for (int b = 0; b < 10; b++)
                        StbGui.stbg_button("Button " + b);
                }
                StbGui.stbg_end_window();
            }
            StbGui.stbg_end_frame();

            StbGui.stbg_render();

        };

        // Scrollbars take 2 frames to appear
        for (int i = 0; i < 2; i++)
            render();

        // use the scroll wheel 3 times
        for (var times = 0; times < 3; times++)
        {
            // use the scroll wheel in the middle of the window
            SetMousePosition(14, 9);
            SetMouseScrollWheelAmount(0, -1);

            render();

            render();
        }

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------------------\""", "MW30"],
            ["""|Window 1                    |""", "MW9MT20MW1"],
            ["""\----------------------------/""", "MW30"],
            ["""|                           ^|""", "BW1BT27BW2"],
            ["""|                            |""", "BW1BT27CT1BW1"],
            ["""| \----------/               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| /----------\               |""", "BW1BT1CB12BT15BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT15BW1"],
            ["""| | Button 2 |               |""", "BW1BT1CB1CT1CW8CT1CB1BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| \----------/               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| /----------\               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| | Button 3 |               |""", "BW1BT1CB1CT1CW8CT1CB1BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| \----------/               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| /----------\               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""|                           v|""", "BW1BT27BW2"],
            ["""\----------------------------/""", "BW30"],
        ]);
    }

    [Fact]
    public void TestScrollWindowUsingMouseWheelWithTenButtonsInsideWhileHoveringAButton()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 20);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 30);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_ALLOW_SCROLLBARS, true);

        Action render = () =>
        {
            StbGui.stbg_begin_frame();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    for (int b = 0; b < 10; b++)
                        StbGui.stbg_button("Button " + b);
                }
                StbGui.stbg_end_window();
            }
            StbGui.stbg_end_frame();

            StbGui.stbg_render();

        };

        // Scrollbars take 2 frames to appear
        for (int i = 0; i < 2; i++)
            render();

        // use the scroll wheel 3 times
        for (var times = 0; times < 3; times++)
        {
            // use the scroll wheel in the middle of the window
            SetMousePosition(6, 9);
            SetMouseScrollWheelAmount(0, -1);

            render();

            render();
        }

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------------------\""", "MW30"],
            ["""|Window 1                    |""", "MW9MT20MW1"],
            ["""\----------------------------/""", "MW30"],
            ["""|                           ^|""", "BW1BT27BW2"],
            ["""|                            |""", "BW1BT27CT1BW1"],
            ["""| \----------/               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| /----------\               |""", "BW1BT1BW12BT15BW1"],
            ["""| |          |               |""", "BW1BT1BW1BT10BW1BT15BW1"],
            ["""| | Button 2 |               |""", "BW1BT1BW1BT1BK8BT1BW1BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1BW1BT10BW1BT14CT1BW1"],
            ["""| \----------/               |""", "BW1BT1BW12BT14CT1BW1"],
            ["""| /----------\               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| | Button 3 |               |""", "BW1BT1CB1CT1CW8CT1CB1BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""| \----------/               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| /----------\               |""", "BW1BT1CB12BT14CT1BW1"],
            ["""| |          |               |""", "BW1BT1CB1CT10CB1BT14CT1BW1"],
            ["""|                           v|""", "BW1BT27BW2"],
            ["""\----------------------------/""", "BW30"],
        ]);
    }

    [Fact]
    public void TestRenderWindowScrollableWithTenButtonsHorizontallyInside()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 20);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 30);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_ALLOW_SCROLLBARS, true);

        // Scrollbars take 2 frames to appear
        for (int i = 0; i < 2; i++)
        {
            StbGui.stbg_begin_frame();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    StbGui.stbg_begin_container("con", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL);
                    {
                        for (int b = 0; b < 10; b++)
                            StbGui.stbg_button("Button " + b);
                    }
                    StbGui.stbg_end_container();
                }
                StbGui.stbg_end_window();
            }
            StbGui.stbg_end_frame();

            StbGui.stbg_render();
        }

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------------------\""", "MW30"],
            ["""|Window 1                    |""", "MW9MT20MW1"],
            ["""\----------------------------/""", "MW30"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""| /----------\/----------\/- |""", "BW1BT1CB26BT1BW1"],
            ["""| |          ||          ||  |""", "BW1BT1CB1CT10CB2CT10CB2CT1BT1BW1"],
            ["""| | Button 0 || Button 1 ||  |""", "BW1BT1CB1CT1CW8CT1CB2CT1CW8CT1CB2CT1BT1BW1"],
            ["""| |          ||          ||  |""", "BW1BT1CB1CT10CB2CT10CB2CT1BT1BW1"],
            ["""| \----------/\----------/\- |""", "BW1BT1CB26BT1BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|<                          >|""", "BW2BT2CT24BW2"],
            ["""\----------------------------/""", "BW30"],
        ]);
    }

    [Fact]
    public void TestScrollWindowWithTenButtonsHorizontallyInside()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 20);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 30);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_ALLOW_SCROLLBARS, true);

        Action render = () =>
        {
            StbGui.stbg_begin_frame();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    StbGui.stbg_begin_container("con", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL);
                    {
                        for (int b = 0; b < 10; b++)
                            StbGui.stbg_button("Button " + b);
                    }
                    StbGui.stbg_end_container();
                }
                StbGui.stbg_end_window();
            }
            StbGui.stbg_end_frame();

            StbGui.stbg_render();

        };

        // Scrollbars take 2 frames to appear
        for (int i = 0; i < 2; i++)
            render();

        // click the "scroll down" button 3 times
        for (var times = 0; times < 3; times++)
        {
            // click the "scroll down" button
            SetMousePosition(28, 18);
            SetMouseButton1(true);

            render();

            // release the "scroll down" button
            SetMouseButton1(false);

            render();
        }

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------------------\""", "WW30"],
            ["""|Window 1                    |""", "WW1WM8WT20WW1"],
            ["""\----------------------------/""", "WW30"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""| --\/----------\/---------- |""", "BW1BT1CB26BT1BW1"],
            ["""|   ||          ||           |""", "BW1BT1CT2CB2CT10CB2CT10BT1BW1"],
            ["""| 0 || Button 1 || Button 2  |""", "BW1BT1CW1CT1CB2CT1CW8CT1CB2CT1CW8CT1BT1BW1"],
            ["""|   ||          ||           |""", "BW1BT1CT2CB2CT10CB2CT10BT1BW1"],
            ["""| --/\----------/\---------- |""", "BW1BT1CB26BT1BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|<                          >|""", "BW2CT2BT2CT22BR1BW1"],
            ["""\----------------------------/""", "BW30"],
        ]);
    }

    [Fact]
    public void TestScrollWindowUsingMouseWheelWithTenButtonsHorizontallyInside()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 20);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 30);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_ALLOW_SCROLLBARS, true);

        Action render = () =>
        {
            StbGui.stbg_begin_frame();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    StbGui.stbg_begin_container("con", StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL);
                    {
                        for (int b = 0; b < 10; b++)
                            StbGui.stbg_button("Button " + b);
                    }
                    StbGui.stbg_end_container();
                }
                StbGui.stbg_end_window();
            }
            StbGui.stbg_end_frame();

            StbGui.stbg_render();

        };

        // Scrollbars take 2 frames to appear
        for (int i = 0; i < 2; i++)
            render();

        // use the scroll wheel 3 times
        for (var times = 0; times < 3; times++)
        {
            // use the scroll wheel 
            SetMousePosition(14, 13);
            SetMouseScrollWheelAmount(1, 0);

            render();

            render();
        }

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------------------\""", "MW30"],
            ["""|Window 1                    |""", "MW9MT20MW1"],
            ["""\----------------------------/""", "MW30"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""| --\/----------\/---------- |""", "BW1BT1CB26BT1BW1"],
            ["""|   ||          ||           |""", "BW1BT1CT2CB2CT10CB2CT10BT1BW1"],
            ["""| 0 || Button 1 || Button 2  |""", "BW1BT1CW1CT1CB2CT1CW8CT1CB2CT1CW8CT1BT1BW1"],
            ["""|   ||          ||           |""", "BW1BT1CT2CB2CT10CB2CT10BT1BW1"],
            ["""| --/\----------/\---------- |""", "BW1BT1CB26BT1BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""|<                          >|""", "BW2CT2BT2CT22BW2"],
            ["""\----------------------------/""", "BW30"],
        ]);
    }

    [Fact]
    public void TestRenderWindowScrollableWithBothScrollDirections()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_HEIGHT, 20);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_DEFAULT_WIDTH, 30);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_ALLOW_SCROLLBARS, true);

        // Scrollbars take 2 frames to appear
        for (int i = 0; i < 2; i++)
        {
            StbGui.stbg_begin_frame();
            {
                StbGui.stbg_begin_window("Window 1");
                {
                    for (int c = 0; c < 10; c++)
                    {
                        StbGui.stbg_begin_container("con" + c, StbGui.STBG_CHILDREN_LAYOUT.HORIZONTAL);
                        {
                            for (int b = 0; b < 10; b++)
                                StbGui.stbg_button("Button " + b);
                        }
                        StbGui.stbg_end_container();
                    }
                }
                StbGui.stbg_end_window();
            }
            StbGui.stbg_end_frame();

            StbGui.stbg_render();
        }

        RenderCommandsToTestScreen();

        AssertScreenEqual([
            ["""/----------------------------\""", "MW30"],
            ["""|Window 1                    |""", "MW9MT20MW1"],
            ["""\----------------------------/""", "MW30"],
            ["""|                           ^|""", "BW1BT27BW2"],
            ["""|                            |""", "BW1BT28BW1"],
            ["""| /----------\/----------\/  |""", "BW1BT1CB25BT2BW1"],
            ["""| |          ||          ||  |""", "BW1BT1CB1CT10CB2CT10CB2BT1CT1BW1"],
            ["""| | Button 0 || Button 1 ||  |""", "BW1BT1CB1CT1CW8CT1CB2CT1CW8CT1CB2BT1CT1BW1"],
            ["""| |          ||          ||  |""", "BW1BT1CB1CT10CB2CT10CB2BT1CT1BW1"],
            ["""| \----------/\----------/\  |""", "BW1BT1CB25BT1CT1BW1"],
            ["""| /----------\/----------\/  |""", "BW1BT1CB25BT1CT1BW1"],
            ["""| |          ||          ||  |""", "BW1BT1CB1CT10CB2CT10CB2BT1CT1BW1"],
            ["""| | Button 0 || Button 1 ||  |""", "BW1BT1CB1CT1CW8CT1CB2CT1CW8CT1CB2BT1CT1BW1"],
            ["""| |          ||          ||  |""", "BW1BT1CB1CT10CB2CT10CB2BT1CT1BW1"],
            ["""| \----------/\----------/\  |""", "BW1BT1CB25BT1CT1BW1"],
            ["""| /----------\/----------\/  |""", "BW1BT1CB25BT1CT1BW1"],
            ["""| |          ||          ||  |""", "BW1BT1CB1CT10CB2CT10CB2BT1CT1BW1"],
            ["""|                            |""", "BW1BT27CT1BW1"],
            ["""|<                         >v|""", "BW2BT2CT23BW3"],
            ["""\----------------------------/""", "BW30"],
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
        SetMousePosition(8, 10);

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
        SetMousePosition(2, 2);
        SetMouseButton1(true);

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
        SetMouseButton1(false);

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
        SetMousePosition(2, 2);
        SetMouseButton1(true);

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
        SetMousePosition(4, 2);

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
        SetMousePosition(6, 2);
        SetMouseButton1(false);

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