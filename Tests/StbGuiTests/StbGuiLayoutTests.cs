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

        Assert.Equal(new StbGui.stbg_size() { width = "Hello World".Length + 4, height = 5 }, StbGui.stbg_get_widget_by_id(buttonId).computed_bounds.size);
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

        Assert.Equal(new StbGui.stbg_size() { width = "Hello World".Length + 2, height = 3 }, StbGui.stbg_get_widget_by_id(buttonId).computed_bounds.size);
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

        Assert.Equal(new StbGui.stbg_size() { width = "Hello World".Length, height = 1 }, StbGui.stbg_get_widget_by_id(buttonId).computed_bounds.size);
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

        Assert.Equal(new StbGui.stbg_size() { width = 34, height = 22 }, StbGui.stbg_get_widget_by_id(windowId).computed_bounds.size);
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

        Assert.Equal(new StbGui.stbg_size() { width = 32, height = 20 }, StbGui.stbg_get_widget_by_id(windowId).computed_bounds.size);
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

        Assert.Equal(new StbGui.stbg_size() { width = 32, height = 19 }, StbGui.stbg_get_widget_by_id(windowId).computed_bounds.size);
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

        Assert.Equal(new StbGui.stbg_size() { width = 30, height = 15 }, StbGui.stbg_get_widget_by_id(windowId).computed_bounds.size);
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
        Assert.Equal(new StbGui.stbg_position() { x = 0, y = 0 }, StbGui.stbg_get_widget_by_id(windowId).computed_bounds.relative_position);

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
        Assert.Equal(new StbGui.stbg_position() { x = 30, y = 40 }, StbGui.stbg_get_widget_by_id(windowId).computed_bounds.relative_position);
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
        Assert.Equal(new StbGui.stbg_size() { width = 34, height = 22 }, StbGui.stbg_get_widget_by_id(windowId).computed_bounds.size);

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

        // Validate new position
        Assert.Equal(new StbGui.stbg_size() { width = 70, height = 60 }, StbGui.stbg_get_widget_by_id(windowId).computed_bounds.size);
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
        Assert.Equal(new StbGui.stbg_position() { x = 0, y = 0 }, StbGui.stbg_get_widget_by_id(window1Id).computed_bounds.relative_position);
        Assert.Equal(new StbGui.stbg_position() { x = 0, y = 0 }, StbGui.stbg_get_widget_by_id(window2Id).computed_bounds.relative_position);

        StbGui.stbg_begin_frame();
        {
            StbGui.stbg_begin_window("Window 1");
            {
                window1Id = StbGui.stbg_get_last_widget_id();
                StbGui.stbg_move_window(window1Id, 30, 40);
            }
            StbGui.stbg_end_window();

            StbGui.stbg_begin_window("Window 2");
            {
                window2Id = StbGui.stbg_get_last_widget_id();
                StbGui.stbg_move_window(window2Id, 100, 120);
            }
            StbGui.stbg_end_window();
        }
        StbGui.stbg_end_frame();

        // Validate new positions
        Assert.Equal(new StbGui.stbg_position() { x = 30, y = 40 }, StbGui.stbg_get_widget_by_id(window1Id).computed_bounds.relative_position);
        Assert.Equal(new StbGui.stbg_position() { x = 100, y = 120 }, StbGui.stbg_get_widget_by_id(window2Id).computed_bounds.relative_position);
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

        Assert.Equal(new StbGui.stbg_position() { x = 2, y = 5 }, StbGui.stbg_get_widget_by_id(buttonId).computed_bounds.relative_position);
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

        Assert.Equal(new StbGui.stbg_position() { x = 2, y = 5 }, StbGui.stbg_get_widget_by_id(button1Id).computed_bounds.relative_position);
        Assert.Equal(new StbGui.stbg_position() { x = 2, y = 5 + 5 }, StbGui.stbg_get_widget_by_id(button2Id).computed_bounds.relative_position);
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

        Assert.Equal(new StbGui.stbg_size() { width = 0, height = 0 }, StbGui.stbg_get_widget_by_id(containerId).computed_bounds.size);
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

        Assert.Equal(new StbGui.stbg_size() { width = "Button 1".Length + 4, height = 5 }, StbGui.stbg_get_widget_by_id(buttonId).computed_bounds.size);
        Assert.Equal(StbGui.stbg_get_widget_by_id(buttonId).computed_bounds.size, StbGui.stbg_get_widget_by_id(containerId).computed_bounds.size);
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

        Assert.Equal(new StbGui.stbg_position() { x = 2, y = 5 }, StbGui.stbg_get_widget_by_id(containerId).computed_bounds.relative_position);
        Assert.Equal(new StbGui.stbg_position() { x = 0, y = 0 }, StbGui.stbg_get_widget_by_id(button1Id).computed_bounds.relative_position);
        Assert.Equal(new StbGui.stbg_position() { x = 12, y = 0 }, StbGui.stbg_get_widget_by_id(button2Id).computed_bounds.relative_position);
    }
}
