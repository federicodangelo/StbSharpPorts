using StbSharp;

namespace StbSharp.Tests;

[Collection("Sequential")]
public class StbGuiTestsBase : IDisposable
{
    public void Dispose()
    {
        AssertHierarchyConsistency();
        DestroyGui();
    }

    static protected void InitGUI()
    {
        StbGui.stbg_init(new() { assert_behaviour = StbGui.STBG_ASSERT_BEHAVIOUR.EXCEPTION });
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
}