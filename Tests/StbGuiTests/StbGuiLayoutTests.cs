namespace StbSharp.Tests;

public class StbGuiLayoutTests : StbGuiTestsBase
{
    [Fact]
    public void TestButtonLayout()
    {
        InitGUI();

        int buttonId;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Hello World");
            buttonId = StbGui.stbg_get_last_widget_id();
        }
        StbGui.stbg_end_frame();

        AssertWidgetPosition(buttonId, 0, 0);
        AssertWidgetSize(buttonId, "Hello World".Length + 4, 5);
        AssertWidgetGlobalRect(buttonId, 0, 0, "Hello World".Length + 4, 5);
    }

    [Fact]
    public void TestSetAndGetStyle()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, 333.33f);

        Assert.Equal(333.33f, StbGui.stbg_get_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE));
    }

    [Fact]
    public void TestSetStyleWhildInsideAFrameShouldThrowAnException()
    {
        InitGUI();

        var originalStyle = StbGui.stbg_get_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE);

        StbGui.stbg_begin_frame();
        {
            Assert.Throws<StbGui.StbgAssertException>(() => StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, 333.33f));
            Assert.Throws<StbGui.StbgAssertException>(() => StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, StbGui.stbg_build_color(30, 30, 30)));
        }
        StbGui.stbg_end_frame();

        Assert.Equal(originalStyle, StbGui.stbg_get_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE));
    }

    [Fact]
    public void TestButtonLayoutWithNoPadding()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PADDING_TOP, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PADDING_BOTTOM, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PADDING_RIGHT, 0);

        int buttonId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Hello World");
            buttonId = StbGui.stbg_get_last_widget_id();
        }
        StbGui.stbg_end_frame();

        AssertWidgetSize(buttonId, "Hello World".Length + 2, 3);
    }

    [Fact]
    public void TestButtonLayoutWithNoPaddingAndNoBorder()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_BORDER_SIZE, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PADDING_TOP, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PADDING_BOTTOM, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PADDING_LEFT, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.BUTTON_PADDING_RIGHT, 0);

        int buttonId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_button("Hello World");
            buttonId = StbGui.stbg_get_last_widget_id();
        }
        StbGui.stbg_end_frame();

        AssertWidgetSize(buttonId, "Hello World".Length, 1);
    }

    [Fact]
    public void TestWindowLayout()
    {
        InitGUI();

        int windowId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Hello World");
            {
                windowId = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        AssertWidgetSize(windowId, 34, 22);
    }

    [Fact]
    public void TestWindowLayoutNoBorder()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, 0);

        int windowId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Hello World");
            {
                windowId = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        AssertWidgetSize(windowId, 34, 22);
    }

    [Fact]
    public void TestWindowLayoutNoBorderNoTitle()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, 0);

        int windowId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Hello World");
            {
                windowId = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        AssertWidgetSize(windowId, 34, 22);
    }

    [Fact]
    public void TestWindowLayoutNoBorderNoTitleNoPadding()
    {
        InitGUI();

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_BORDER_SIZE, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_HEIGHT, 0);

        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_TOP, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_BOTTOM, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_LEFT, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_TITLE_PADDING_RIGHT, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_TOP, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_BOTTOM, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_LEFT, 0);
        StbGui.stbg_set_widget_style(StbGui.STBG_WIDGET_STYLE.WINDOW_CHILDREN_PADDING_RIGHT, 0);

        int windowId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Hello World");
            {
                windowId = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        AssertWidgetSize(windowId, 34, 22);
    }

    [Fact]
    public void TestWindowLayoutMoveWindow()
    {
        InitGUI();

        int windowId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Hello World");
            {
                windowId = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        // Validate initial position
        AssertWidgetPosition(windowId, 0, 0);
        AssertWidgetGlobalRect(windowId, 0, 0, 34, 22);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Hello World");
            {
                windowId = StbGui.stbg_get_last_widget_id();
                StbGui.stbg_move_window(windowId, 30, 40);
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        // Validate new position
        AssertWidgetPosition(windowId, 30, 40);
        AssertWidgetGlobalRect(windowId, 30, 40, 30 + 34, 40 + 22);
    }

    [Fact]
    public void TestWindowLayoutMoveTwoWindows()
    {
        InitGUI();

        int window1Id, window2Id;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                window1Id = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();


            StbGui.stbg_begin_window("Window 2");
            {
                window2Id = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        // Validate initial positions
        AssertWidgetPosition(window1Id, 0, 0);
        AssertWidgetGlobalRect(window1Id, 0, 0, 34, 22);
        AssertWidgetPosition(window2Id, 1, 1);
        AssertWidgetGlobalRect(window2Id, 1, 1, 35, 23);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                window1Id = StbGui.stbg_get_last_widget_id();
                StbGui.stbg_move_window(window1Id, 10, 20);
            }
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            {
                window2Id = StbGui.stbg_get_last_widget_id();
                StbGui.stbg_move_window(window2Id, 30, 40);
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        // Validate new positions
        AssertWidgetPosition(window1Id, 10, 20);
        AssertWidgetGlobalRect(window1Id, 10, 20, 10 + 34, 20 + 22);
        AssertWidgetPosition(window2Id, 30, 40);
        AssertWidgetGlobalRect(window2Id, 30, 40, 30 + 34, 40 + 22);
    }

    [Fact]
    public void TestWindowLayoutResizeWindow()
    {
        InitGUI();

        int windowId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Hello World");
            {
                windowId = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        // Validate initial size
        AssertWidgetSize(windowId, 34, 22);

        Assert.Equal(new StbGui.stbg_size() { width = 34, height = 22 }, StbGui.stbg_get_widget_by_id(windowId).properties.computed_bounds.size);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Hello World");
            {
                windowId = StbGui.stbg_get_last_widget_id();
                StbGui.stbg_resize_window(windowId, 70, 60);
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        // Validate new size
        AssertWidgetSize(windowId, 70, 60);
    }

    [Fact]
    public void TestWindowWithButtonInsideLayout()
    {
        InitGUI();

        int buttonId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Hello World");
            {
                StbGui.stbg_button("Button 1");
                buttonId = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        AssertWidgetPosition(buttonId, 2, 5);
    }

    [Fact]
    public void TestWindowWithTwoButtonsInsideLayout()
    {
        InitGUI();

        int button1Id, button2Id;

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Hello World");
            {
                StbGui.stbg_button("Button 1");
                button1Id = StbGui.stbg_get_last_widget_id();

                StbGui.stbg_button("Button 2");
                button2Id = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        AssertWidgetPosition(button1Id, 2, 5);
        AssertWidgetPosition(button2Id, 2, 5 + 5);
    }

    [Fact]
    public void TestContainerEmptyLayout()
    {
        InitGUI();

        int containerId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_container("Container 1", StbGui.STBG_CHILDREN_LAYOUT_DIRECTION.HORIZONTAL);
            {
                containerId = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_container();
        }

        AssertWidgetPosition(containerId, 0, 0);
        AssertWidgetSize(containerId, 0, 0);
        AssertWidgetGlobalRect(containerId, 0, 0, 0, 0);
    }

    [Fact]
    public void TestContainerWithSingleButtonLayout()
    {
        InitGUI();

        int buttonId, containerId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_container("Container 1", StbGui.STBG_CHILDREN_LAYOUT_DIRECTION.HORIZONTAL);
            {
                containerId = StbGui.stbg_get_last_widget_id();

                StbGui.stbg_button("Button 1");
                buttonId = StbGui.stbg_get_last_widget_id();
            }
            StbGui.stbg_end_container();
        }
        StbGui.stbg_end_frame();

        AssertWidgetSize(buttonId, "Button 1".Length + 4, 5);
        AssertWidgetSize(containerId, "Button 1".Length + 4, 5);
    }

    [Fact]
    public void TestContainerWithTwoButtonsInHorizontalLayout()
    {
        InitGUI();

        int button1Id, button2Id, containerId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_container("Container 1", StbGui.STBG_CHILDREN_LAYOUT_DIRECTION.HORIZONTAL);
            {
                containerId = StbGui.stbg_get_last_widget_id();

                StbGui.stbg_button("Button 1");
                button1Id = StbGui.stbg_get_last_widget_id();

                StbGui.stbg_button("Button 2");
                button2Id = StbGui.stbg_get_last_widget_id();
            }

            StbGui.stbg_end_container();
        }
        StbGui.stbg_end_frame();

        // Container is top level
        AssertWidgetPosition(containerId, 0, 0);
        AssertWidgetGlobalRect(containerId, 0, 0, 24, 5);
        // Buttons are inside the container
        AssertWidgetPosition(button1Id, 0, 0);
        AssertWidgetGlobalRect(button1Id, 0, 0, 12, 5);
        AssertWidgetPosition(button2Id, 12, 0);
        AssertWidgetGlobalRect(button2Id, 12, 0, 24, 5);
    }

    [Fact]
    public void TestContainerWithSpacingWithTwoButtonsInHorizontalLayout()
    {
        InitGUI();

        int button1Id, button2Id, containerId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_container("Container 1", StbGui.STBG_CHILDREN_LAYOUT_DIRECTION.HORIZONTAL, 1);
            {
                containerId = StbGui.stbg_get_last_widget_id();

                StbGui.stbg_button("Button 1");
                button1Id = StbGui.stbg_get_last_widget_id();

                StbGui.stbg_button("Button 2");
                button2Id = StbGui.stbg_get_last_widget_id();
            }

            StbGui.stbg_end_container();
        }
        StbGui.stbg_end_frame();

        // Container is top level
        AssertWidgetPosition(containerId, 0, 0);
        AssertWidgetGlobalRect(containerId, 0, 0, 25, 5);
        // Buttons are inside the container
        AssertWidgetPosition(button1Id, 0, 0);
        AssertWidgetGlobalRect(button1Id, 0, 0, 12, 5);
        AssertWidgetPosition(button2Id, 13, 0);
        AssertWidgetGlobalRect(button2Id, 13, 0, 25, 5);
    }

    [Fact]
    public void TestContainerWithTwoButtonsInVerticalLayout()
    {
        InitGUI();

        int button1Id, button2Id, containerId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_container("Container 1", StbGui.STBG_CHILDREN_LAYOUT_DIRECTION.VERTICAL);
            {
                containerId = StbGui.stbg_get_last_widget_id();

                StbGui.stbg_button("Button 1");
                button1Id = StbGui.stbg_get_last_widget_id();

                StbGui.stbg_button("Button 2");
                button2Id = StbGui.stbg_get_last_widget_id();
            }

            StbGui.stbg_end_container();
        }
        StbGui.stbg_end_frame();

        // Container is top level
        AssertWidgetPosition(containerId, 0, 0);
        AssertWidgetGlobalRect(containerId, 0, 0, 12, 10);
        // Buttons are inside the container
        AssertWidgetPosition(button1Id, 0, 0);
        AssertWidgetGlobalRect(button1Id, 0, 0, 12, 5);
        AssertWidgetPosition(button2Id, 0, 5);
        AssertWidgetGlobalRect(button2Id, 0, 5, 12, 10);
    }

    [Fact]
    public void TestContainerWithSpacingWithTwoButtonsInVerticalLayout()
    {
        InitGUI();

        int button1Id, button2Id, containerId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_container("Container 1", StbGui.STBG_CHILDREN_LAYOUT_DIRECTION.VERTICAL, 1);
            {
                containerId = StbGui.stbg_get_last_widget_id();

                StbGui.stbg_button("Button 1");
                button1Id = StbGui.stbg_get_last_widget_id();

                StbGui.stbg_button("Button 2");
                button2Id = StbGui.stbg_get_last_widget_id();
            }

            StbGui.stbg_end_container();
        }
        StbGui.stbg_end_frame();

        // Container is top level
        AssertWidgetPosition(containerId, 0, 0);
        AssertWidgetGlobalRect(containerId, 0, 0, 12, 11);
        // Buttons are inside the container
        AssertWidgetPosition(button1Id, 0, 0);
        AssertWidgetGlobalRect(button1Id, 0, 0, 12, 5);
        AssertWidgetPosition(button2Id, 0, 6);
        AssertWidgetGlobalRect(button2Id, 0, 6, 12, 11);
    }

    [Fact]
    public void TestWindowWithTwoButtonsInHorizontalContainerInsideLayout()
    {
        InitGUI();

        int button1Id, button2Id, containerId;
        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Hello World");
            {
                StbGui.stbg_begin_container("Container 1", StbGui.STBG_CHILDREN_LAYOUT_DIRECTION.HORIZONTAL);
                {
                    containerId = StbGui.stbg_get_last_widget_id();

                    StbGui.stbg_button("Button 1");
                    button1Id = StbGui.stbg_get_last_widget_id();

                    StbGui.stbg_button("Button 2");
                    button2Id = StbGui.stbg_get_last_widget_id();
                }

                StbGui.stbg_end_container();
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        // Container is inside the window
        AssertWidgetPosition(containerId, 2, 5);
        AssertWidgetGlobalRect(containerId, 2, 5, 26, 10);
        // Buttons are inside the container
        AssertWidgetPosition(button1Id, 0, 0);
        AssertWidgetGlobalRect(button1Id, 2, 5, 14, 10);
        AssertWidgetPosition(button2Id, 12, 0);
        AssertWidgetGlobalRect(button2Id, 14, 5, 26, 10);
    }
}
