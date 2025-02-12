using StbSharp;

namespace StbSharp.Tests;

[Collection("Sequential")]
public class StbGuiTestsBase : IDisposable
{
    protected const float ScreenSizeWidth = 512;
    protected const float ScreenSizeHeight = 256;

    public void Dispose()
    {
        AssertHierarchyConsistency();
        DestroyGui();
    }

    static private StbGui.stbg_external_dependencies BuildExternalDependencies()
    {
        return new StbGui.stbg_external_dependencies()
        {
            measure_text = (text, font, style) => new StbGui.stbg_size() { width = text.Length * style.size, height = style.size }
        };
    }

    static protected void InitGUI()
    {
        InitGUI(new() { assert_behaviour = StbGui.STBG_ASSERT_BEHAVIOUR.EXCEPTION });
    }

    static protected void InitGUI(StbGui.stbg_init_options options)
    {
        StbGui.stbg_init(BuildExternalDependencies(), options);
        StbGui.stbg_set_screen_size(ScreenSizeWidth, ScreenSizeHeight);

        int fontId = StbGui.stbg_add_font("default_font");

        StbGui.stbg_init_default_theme(
            fontId,
            new() { size = 1, style = StbGui.STBG_FONT_STYLE_FLAGS.NONE }
        );
    }

    static protected void DestroyGui()
    {
        StbGui.stbg_destroy();
    }

    static protected void AssertGuiNotInitialized()
    {
        Assert.Null(StbGui.stbg_get_context().widgets);
    }

    static protected void AssertGuiInitialized()
    {
        Assert.NotNull(StbGui.stbg_get_context().widgets);
    }

    static protected void AssertHierarchyConsistency()
    {
        var root_widget_id = StbGui.stbg_get_context().root_widget_id;

        if (root_widget_id != StbGui.STBG_WIDGET_ID_NULL)
        {
            ValidateHierarchy(root_widget_id);
        }
    }

    static void ValidateHierarchy(int widget_id)
    {
        var widget = StbGui.stbg_get_widget_by_id(widget_id);

        // Validate that all next siblings point to the same parent, and that the next <-> previous chain is ok
        {
            var nextSiblingId = widget.hierarchy.next_sibling_id;
            var previousSiblingId = widget.id;

            while (nextSiblingId != StbGui.STBG_WIDGET_ID_NULL)
            {
                var nextSibling = StbGui.stbg_get_widget_by_id(nextSiblingId);

                Assert.Equal(widget.hierarchy.parent_id, nextSibling.hierarchy.parent_id);
                Assert.Equal(previousSiblingId, nextSibling.hierarchy.prev_sibling_id);

                nextSiblingId = nextSibling.hierarchy.next_sibling_id;
                previousSiblingId = nextSibling.id;
            }
        }

        // Validate that all previous siblings point to the same parent, and that the next <-> previous chain is ok
        {
            var nextSiblingId = widget.id;
            var previousSiblingId = widget.hierarchy.prev_sibling_id;

            while (previousSiblingId != StbGui.STBG_WIDGET_ID_NULL)
            {
                var previousSibling = StbGui.stbg_get_widget_by_id(previousSiblingId);

                Assert.Equal(widget.hierarchy.parent_id, previousSibling.hierarchy.parent_id);
                Assert.Equal(nextSiblingId, previousSibling.hierarchy.next_sibling_id);

                nextSiblingId = previousSibling.id;
                previousSiblingId = previousSibling.hierarchy.prev_sibling_id;
            }
        }

        // Validate all children
        {
            var childrenId = widget.hierarchy.first_children_id;
            while (childrenId != StbGui.STBG_WIDGET_ID_NULL)
            {
                var children = StbGui.stbg_get_widget_by_id(childrenId);

                Assert.Equal(widget.id, children.hierarchy.parent_id);

                ValidateHierarchy(childrenId);

                childrenId = children.hierarchy.next_sibling_id;
            }
        }
    }

    static protected void AssertWidgetSize(int widget_id, float width, float height)
    {
        Assert.Equal(
            new StbGui.stbg_size() { width = width, height = height },
            StbGui.stbg_get_widget_by_id(widget_id).computed_bounds.size
        );
    }

    static protected void AssertWidgetPosition(int widget_id, float x, float y)
    {
        Assert.Equal(
            new StbGui.stbg_position() { x = x, y = y },
            StbGui.stbg_get_widget_by_id(widget_id).computed_bounds.relative_position
        );
    }

    static protected void AssertWidgetGlobalRect(int widget_id, float x0, float y0, float x1, float y1)
    {
        Assert.Equal(
            new StbGui.stbg_rect() { top_left = new StbGui.stbg_position() { x = x0, y = y0 }, bottom_right = new StbGui.stbg_position() { x = x1, y = y1 } },
            StbGui.stbg_get_widget_by_id(widget_id).computed_bounds.global_rect
        );
    }
}