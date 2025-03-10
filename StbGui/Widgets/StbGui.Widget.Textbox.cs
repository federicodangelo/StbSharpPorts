#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;
using System.Diagnostics;

public partial class StbGui
{
    private struct stbg__textbox_parameters
    {
        public bool single_line;
    }

    private static void stbg__textbox_init_default_theme()
    {
        var font_style = context.theme.default_font_style;

        // TEXTBOX
        var textboxPaddingTopBottom = MathF.Ceiling(font_style.size / 4);
        var textboxPaddingLeftRight = MathF.Ceiling(font_style.size / 4);

        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP, textboxPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_PADDING_BOTTOM, textboxPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT, textboxPaddingLeftRight);
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_PADDING_RIGHT, textboxPaddingLeftRight);

        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_BACKGROUND_COLOR, rgb(236, 240, 241));
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_BORDER_COLOR, rgb(52, 73, 94));
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_BORDER_SIZE, 1);
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_TEXT_COLOR, rgb(44, 62, 80));
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_COLOR, rgb(44, 62, 80));
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_WIDTH, 1);
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_HEIGHT, font_style.size + 2);
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_BLINKING_RATE, 500);
    }

    private static void stbg__textbox_init_context(ref stbg_context context)
    {
        context.text_edit.state.undostate = new StbTextEdit.StbUndoState();
        context.text_edit.textbox_style_ranges = new stbg_render_text_style_range[3];

        ref var str = ref context.text_edit.str;
        str.get_width = stbg__textbox_text_edit_get_width;
        str.layout_row = stbg__textbox_text_edit_layout_row;
    }

    private static void stbg__textbox_set_parameters(ref stbg_widget widget, stbg__textbox_parameters parameters)
    {
        widget.properties.parameters.parameter1.b = parameters.single_line;
    }

    private static void stbg__textbox_get_parameters(ref stbg_widget widget, out stbg__textbox_parameters parameters)
    {
        parameters = new stbg__textbox_parameters();
        parameters.single_line = widget.properties.parameters.parameter1.b;
    }

    private static ref stbg_widget stbg__textbox_create(ReadOnlySpan<char> identifier, ref stbg_textbox_text_to_edit text_to_edit, int visible_lines)
    {
        ref var textbox = ref stbg__add_widget(STBG_WIDGET_TYPE.TEXTBOX, identifier, out var is_new);
        ref var textbox_ref_props = ref stbg__get_widget_ref_props_by_id_internal(textbox.id);

        var parameters = new stbg__textbox_parameters()
        {
            single_line = visible_lines == 1
        };

        textbox_ref_props.text_to_edit.text = text_to_edit.text;

        stbg__textbox_set_parameters(ref textbox, parameters);

        ref var layout = ref textbox.properties.layout;

        layout.constrains = stbg_build_constrains_unconstrained();
        layout.constrains.min.height = context.theme.default_font_style.size * visible_lines + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP, STBG_WIDGET_STYLE.TEXTBOX_PADDING_BOTTOM);
        layout.constrains.min.width = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT, STBG_WIDGET_STYLE.TEXTBOX_PADDING_RIGHT);
        layout.inner_padding = new stbg_padding()
        {
            top = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP),
            bottom = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_BOTTOM),
            left = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT),
            right = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_RIGHT),
        };
        layout.intrinsic_size.size = stbg_build_size(300, layout.constrains.min.height);

        if (is_new || (textbox.properties.input_flags & STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED) == 0)
        {
            textbox_ref_props.text_to_edit.length = text_to_edit.length;
        }
        else
        {
            text_to_edit.length = textbox_ref_props.text_to_edit.length;
            textbox.properties.input_flags &= ~STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
        }

        return ref textbox;
    }

    private static int stbg__textbox_text_edit_get_width(StbTextEdit.STB_TEXTEDIT_STRING str, int n, int i)
    {
        ref var textbox = ref stbg_get_widget_by_id(context.text_edit.widget_id);
        stbg__assert_internal(textbox.type == STBG_WIDGET_TYPE.TEXTBOX);
        stbg__textbox_get_parameters(ref textbox, out var parameters);
        var measure_text_options = parameters.single_line ? STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE : STBG_MEASURE_TEXT_OPTIONS.NONE;

        if (str.text.Span[n + i] == '\n')
            return StbTextEdit.STB_TEXTEDIT_GETWIDTH_NEWLINE;

        var w1 = stbg__get_character_position_in_text(stbg__build_text(str.text.Slice(n)), i, measure_text_options).x;
        var w2 = stbg__get_character_position_in_text(stbg__build_text(str.text.Slice(n)), i + 1, measure_text_options).x;
        return (int)(w2 - w1);
    }

    private static StbTextEdit.StbTexteditRow stbg__textbox_text_edit_layout_row(StbTextEdit.STB_TEXTEDIT_STRING str, int n)
    {
        ref var textbox = ref stbg_get_widget_by_id(context.text_edit.widget_id);
        stbg__assert_internal(textbox.type == STBG_WIDGET_TYPE.TEXTBOX);
        stbg__textbox_get_parameters(ref textbox, out var parameters);
        var single_line = parameters.single_line;
        var measure_text_options = single_line ? STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE : STBG_MEASURE_TEXT_OPTIONS.NONE;

        var row = new StbTextEdit.StbTexteditRow();

        var text = str.text.Slice(0, str.text_length).Slice(n);

        var first_new_line = single_line == false ? text.Span.IndexOf('\n') : -1;
        var has_new_line = false;
        if (first_new_line != -1)
        {
            text = text.Slice(0, first_new_line);
            has_new_line = true;
        }

        var size = stbg__measure_text(stbg__build_text(text), measure_text_options);
        row.x0 = 0;
        row.x1 = size.width;
        row.ymin = 0;
        row.ymax = size.height;
        row.baseline_y_delta = size.height / 2;
        row.num_chars = text.Length + (has_new_line ? 1 : 0);

        return row;
    }

    private static bool stbg__textbox_update_input(ref stbg_widget textbox)
    {
        ref var textbox_ref_props = ref stbg__get_widget_ref_props_by_id_internal(textbox.id);

        if (context.input_feedback.hovered_widget_id == textbox.id &&
            context.input.mouse_button_1_down &&
            context.input_feedback.editing_text_widget_id != textbox.id)
        {
            stbg__textbox_start_edit(ref textbox);
        }

        if (context.input_feedback.editing_text_widget_id == textbox.id)
        {
            ref var str = ref context.text_edit.str;
            ref var state = ref context.text_edit.state;

            if (context.text_edit.widget_id != textbox.id || context.text_edit.widget_hash != textbox.hash)
            {
                context.text_edit.widget_id = textbox.id;
                context.text_edit.widget_hash = textbox.hash;
                stbg__textbox_get_parameters(ref textbox, out var parameters);

                str.text = textbox_ref_props.text_to_edit.text;
                str.text_length = textbox_ref_props.text_to_edit.length;

                StbTextEdit.stb_textedit_initialize_state(ref state, parameters.single_line);
            }

            var stop_editing = false;

            for (int i = 0; i < context.user_input_events_queue_offset; i++)
            {
                var user_event = context.user_input_events_queue[i];

                stop_editing = stbg__textbox_handle_user_event(ref textbox, ref textbox_ref_props, ref str, ref state, user_event);
            }

            if (stop_editing)
            {
                stbg__textbox_stop_edit(ref textbox);
            }
        }

        return false;
    }

    private static void stbg__textbox_start_edit(ref stbg_widget textbox)
    {
        // Start edit
        context.input_feedback.editing_text_widget_id = textbox.id;

        context.input_feedback.ime_info = new stbg_input_method_editor_info()
        {
            widget_id = textbox.id,
            enable = true,
            editing_global_rect = textbox.properties.computed_bounds.global_rect,
            editing_cursor_global_x = textbox.properties.computed_bounds.global_rect.x0,
        };

        context.external_dependencies.set_input_method_editor(new stbg_input_method_editor_info()
        {
            enable = true,
            editing_global_rect = textbox.properties.computed_bounds.global_rect,
            editing_cursor_global_x = textbox.properties.computed_bounds.global_rect.x0
        });

        textbox.properties.layout.children_offset.x = 0;
        textbox.properties.layout.children_offset.y = 0;
    }

    private static void stbg__textbox_stop_edit(ref stbg_widget textbox)
    {
        context.input_feedback.editing_text_widget_id = STBG_WIDGET_ID_NULL;
        context.text_edit.widget_id = STBG_WIDGET_ID_NULL;
        context.text_edit.widget_hash = 0;

        if (context.input_feedback.ime_info.widget_id == textbox.id)
            context.input_feedback.ime_info = new stbg_input_method_editor_info();
    }

    private static bool stbg__textbox_handle_user_event(ref stbg_widget textbox, ref stbg_widget_reference_properties textbox_ref_props, ref StbTextEdit.STB_TEXTEDIT_STRING str, ref StbTextEdit.STB_TexteditState state, stbg_user_input_input_event user_event)
    {
        var stop_editing = false;
        float draw_offset_x = textbox.properties.layout.children_offset.x;
        float draw_offset_y = textbox.properties.layout.children_offset.y;

        switch (user_event.type)
        {
            case STBG_INPUT_EVENT_TYPE.MOUSE_BUTTON:
                if (user_event.mouse_button == 1 && user_event.mouse_button_pressed)
                {
                    if (!stbg_rect_is_position_inside(textbox.properties.computed_bounds.global_rect, context.input.mouse_position))
                    {
                        stop_editing = true;
                        break;
                    }

                    var textbox_local_position = stbg_offset_position(context.input.mouse_position,
                        -textbox.properties.computed_bounds.global_rect.x0 - stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT) - draw_offset_x,
                        -textbox.properties.computed_bounds.global_rect.y0 - stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP) - draw_offset_y
                    );
                    StbTextEdit.stb_textedit_click(ref str, ref state, textbox_local_position.x, textbox_local_position.y);
                }
                break;

            case STBG_INPUT_EVENT_TYPE.MOUSE_POSITION:
                if (context.input.mouse_button_1)
                {
                    var textbox_local_position = stbg_offset_position(context.input.mouse_position,
                        -textbox.properties.computed_bounds.global_rect.x0 - stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT) - draw_offset_x,
                        -textbox.properties.computed_bounds.global_rect.y0 - stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP) - draw_offset_y
                    );
                    StbTextEdit.stb_textedit_drag(ref str, ref state, textbox_local_position.x, textbox_local_position.y);
                }
                break;

            case STBG_INPUT_EVENT_TYPE.KEYBOARD_KEY:
                {
                    int textedit_key = 0;

                    if (user_event.key_pressed)
                    {
                        bool shift = (user_event.key_modifiers & STBG_KEYBOARD_MODIFIER_FLAGS.SHIFT) != 0;
                        bool control = (user_event.key_modifiers & STBG_KEYBOARD_MODIFIER_FLAGS.CONTROL) != 0;
                        bool alt = (user_event.key_modifiers & STBG_KEYBOARD_MODIFIER_FLAGS.ALT) != 0;
                        bool edited = false;

                        switch (user_event.key)
                        {
                            case STBG_KEYBOARD_KEY.CHARACTER:
                                if (control || alt)
                                {
                                    var c = char.ToLowerInvariant(user_event.key_character);

                                    // Special keyboard shortcut, never a normal key
                                    if (control && c == 'a')
                                    {
                                        // Select all
                                        state.select_start = 0;
                                        state.select_end = str.text_length;
                                    }
                                    else if (control && c == 'z')
                                    {
                                        // Undo
                                        textedit_key = StbTextEdit.STB_TEXTEDIT_K_UNDO;
                                    }
                                    else if (control && c == 'y')
                                    {
                                        // Redo
                                        textedit_key = StbTextEdit.STB_TEXTEDIT_K_REDO;
                                    }
                                    else if (control && c == 'c')
                                    {
                                        // Copy
                                        if (state.select_start != state.select_end)
                                        {
                                            var start = Math.Min(state.select_start, state.select_end);
                                            var end = Math.Max(state.select_start, state.select_end);

                                            var text_to_copy = str.text.Slice(start, end - start);

                                            context.external_dependencies.copy_text_to_clipboard(text_to_copy.Span);
                                        }
                                    }
                                    else if (control && c == 'x')
                                    {
                                        // Cut
                                        if (state.select_start != state.select_end)
                                        {
                                            var start = Math.Min(state.select_start, state.select_end);
                                            var end = Math.Max(state.select_start, state.select_end);

                                            var text_to_copy = str.text.Slice(start, end - start);

                                            context.external_dependencies.copy_text_to_clipboard(text_to_copy.Span);

                                            StbTextEdit.stb_textedit_cut(ref str, ref state);
                                        }
                                    }
                                    else if (control && c == 'v')
                                    {
                                        // Paste
                                        var text_to_paste = context.external_dependencies.get_clipboard_text();

                                        if (text_to_paste.Length > 0)
                                        {
                                            StbTextEdit.stb_textedit_paste(ref str, ref state, text_to_paste, text_to_paste.Length);
                                            edited = true;
                                        }
                                    }
                                }
                                else
                                {
                                    textedit_key = user_event.key_character;
                                }
                                break;
                            case STBG_KEYBOARD_KEY.LEFT:
                                if (control)
                                    textedit_key = StbTextEdit.STB_TEXTEDIT_K_WORDLEFT;
                                else
                                    textedit_key = StbTextEdit.STB_TEXTEDIT_K_LEFT;
                                context.text_edit.last_cursor_moved_time = context.current_time_milliseconds;
                                break;
                            case STBG_KEYBOARD_KEY.RIGHT:
                                if (control)
                                    textedit_key = StbTextEdit.STB_TEXTEDIT_K_WORDRIGHT;
                                else
                                    textedit_key = StbTextEdit.STB_TEXTEDIT_K_RIGHT;
                                context.text_edit.last_cursor_moved_time = context.current_time_milliseconds;
                                break;
                            case STBG_KEYBOARD_KEY.UP:
                                textedit_key = StbTextEdit.STB_TEXTEDIT_K_UP;
                                context.text_edit.last_cursor_moved_time = context.current_time_milliseconds;
                                break;
                            case STBG_KEYBOARD_KEY.DOWN:
                                textedit_key = StbTextEdit.STB_TEXTEDIT_K_DOWN;
                                context.text_edit.last_cursor_moved_time = context.current_time_milliseconds;
                                break;
                            case STBG_KEYBOARD_KEY.BACKSPACE:
                                textedit_key = StbTextEdit.STB_TEXTEDIT_K_BACKSPACE;
                                break;
                            case STBG_KEYBOARD_KEY.DELETE:
                                textedit_key = StbTextEdit.STB_TEXTEDIT_K_DELETE;
                                break;
                            case STBG_KEYBOARD_KEY.RETURN:
                                textedit_key = StbTextEdit.STB_TEXTEDIT_NEWLINE;
                                break;
                            case STBG_KEYBOARD_KEY.PAGE_UP:
                                textedit_key = StbTextEdit.STB_TEXTEDIT_K_PGUP;
                                break;
                            case STBG_KEYBOARD_KEY.PAGE_DOWN:
                                textedit_key = StbTextEdit.STB_TEXTEDIT_K_PGDOWN;
                                break;
                            case STBG_KEYBOARD_KEY.HOME:
                                if (control)
                                    textedit_key = StbTextEdit.STB_TEXTEDIT_K_TEXTSTART;
                                else
                                    textedit_key = StbTextEdit.STB_TEXTEDIT_K_LINESTART;
                                break;
                            case STBG_KEYBOARD_KEY.END:
                                if (control)
                                    textedit_key = StbTextEdit.STB_TEXTEDIT_K_TEXTEND;
                                else
                                    textedit_key = StbTextEdit.STB_TEXTEDIT_K_LINEEND;
                                break;
                        }

                        if (textedit_key != 0)
                        {
                            if (shift)
                                textedit_key |= StbTextEdit.STB_TEXTEDIT_K_SHIFT;

                            StbTextEdit.stb_textedit_key(ref str, ref state, textedit_key);

                            edited = true;
                        }

                        if (edited)
                        {
                            textbox_ref_props.text_to_edit.length = str.text_length;
                            textbox.properties.input_flags |= STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
                        }
                    }
                    break;
                }
        }

        return stop_editing;
    }

    private static void stbg__textbox_render(ref stbg_widget textbox)
    {
        ref var textbox_ref_props = ref stbg__get_widget_ref_props_by_id_internal(textbox.id);

        var size = textbox.properties.computed_bounds.size;
        var editing = context.input_feedback.editing_text_widget_id == textbox.id;
        var text = textbox_ref_props.text_to_edit.text.Slice(0, textbox_ref_props.text_to_edit.length);
        stbg__textbox_get_parameters(ref textbox, out var parameters);

        var needs_clipping = editing;

        var text_render_options = needs_clipping ? STBG_RENDER_TEXT_OPTIONS.DONT_CLIP : STBG_RENDER_TEXT_OPTIONS.NONE; // No need to use clipping IF we are already clipping the whole textbox
        var text_measure_options = parameters.single_line ? STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE : STBG_MEASURE_TEXT_OPTIONS.NONE;

        float cursor_x, cursor_y;
        float line_height = context.theme.default_font_style.size;

        // We re-used the children offset values since a textbox can't have children..
        float draw_offset_x = editing ? textbox.properties.layout.children_offset.x : 0;
        float draw_offset_y = editing ? textbox.properties.layout.children_offset.y : 0;

        if (editing)
        {
            var cursor_position = stbg__get_character_position_in_text(stbg__build_text(text.Slice(0, text.Length)), context.text_edit.state.cursor, text_measure_options);

            cursor_x = draw_offset_x + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT) + cursor_position.x;
            cursor_y = draw_offset_y + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP) + cursor_position.y;

            if (cursor_x > size.width - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_RIGHT))
            {
                cursor_x = size.width - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_RIGHT);
                draw_offset_x = cursor_x - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT) - cursor_position.x;

                textbox.properties.layout.children_offset.x = draw_offset_x;
            }
            else if (cursor_x < stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT))
            {
                cursor_x = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT);
                draw_offset_x = cursor_x - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT) - cursor_position.x;
                textbox.properties.layout.children_offset.x = draw_offset_x;
            }

            if (cursor_y + line_height > size.height - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_BOTTOM))
            {
                cursor_y = size.height - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_BOTTOM) - line_height;
                draw_offset_y = cursor_y - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP) - cursor_position.y;
                textbox.properties.layout.children_offset.y = draw_offset_y;
            }
            else if (cursor_y < stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP))
            {
                cursor_y = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP);
                draw_offset_y = cursor_y - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP) - cursor_position.y;
                textbox.properties.layout.children_offset.y = draw_offset_y;
            }
        }
        else
        {
            cursor_x = 0;
            cursor_y = 0;
        }

        float border_size = stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTBOX_BORDER_SIZE);

        stbg__rc_draw_border(
            stbg_build_rect(0, 0, size.width, size.height),
            border_size,
            stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTBOX_BORDER_COLOR),
            stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTBOX_BACKGROUND_COLOR)
        );

        if (needs_clipping)
            stbg__rc_push_clipping_rect(stbg_build_rect(border_size, border_size, size.width - border_size, size.height - border_size));

        if (editing && context.text_edit.state.select_start != context.text_edit.state.select_end)
        {
            // Draw text normally with selection background
            var select_start = Math.Clamp(Math.Min(context.text_edit.state.select_start, context.text_edit.state.select_end), 0, text.Length);
            var select_end = Math.Clamp(Math.Max(context.text_edit.state.select_start, context.text_edit.state.select_end), 0, text.Length);

            var style_ranges = context.text_edit.textbox_style_ranges.Span;

            style_ranges[0].start_index = 0;
            style_ranges[0].text_color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTBOX_TEXT_COLOR);
            style_ranges[0].background_color = STBG_COLOR_TRANSPARENT;
            style_ranges[0].font_style = context.theme.default_font_style.style;

            style_ranges[1].start_index = select_start;
            style_ranges[1].text_color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTBOX_BACKGROUND_COLOR);
            style_ranges[1].background_color = stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_COLOR);
            style_ranges[1].font_style = context.theme.default_font_style.style;

            style_ranges[2] = style_ranges[0];
            style_ranges[2].start_index = select_end;

            stbg__rc_draw_text(
                stbg_build_rect(
                    draw_offset_x + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT),
                    draw_offset_y + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP),
                    size.width - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_RIGHT),
                    size.height - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_BOTTOM)
                ),
                text, context.theme.default_font_id, context.theme.default_font_style.size,
                context.text_edit.textbox_style_ranges,
                -1, -1,
                text_measure_options,
                text_render_options
            );
        }
        else
        {
            // Draw text normally
            stbg__rc_draw_text(
                stbg_build_rect(
                    draw_offset_x + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT),
                    draw_offset_y + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP),
                    size.width - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_RIGHT),
                    size.height - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_BOTTOM)
                ),
                stbg__build_text(text, stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTBOX_TEXT_COLOR)),
                -1, -1,
                text_measure_options,
                text_render_options
            );
        }

        // Draw blinking cursor if we are editing and there is no selection visible (blinking cursor is not visible when there is a selection)
        if (editing && context.text_edit.state.select_start == context.text_edit.state.select_end)
        {
            var blinking_rate = Math.Max((int)stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_BLINKING_RATE), 100);
            var blinking_state_visible = (((context.current_time_milliseconds - context.text_edit.last_cursor_moved_time) % (blinking_rate * 2)) / blinking_rate) == 0;

            stbg__enqueue_force_render(ref textbox, blinking_rate);

            if (blinking_state_visible)
            {
                var cursor_y_offset = (line_height - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_HEIGHT)) / 2;

                stbg__rc_draw_rectangle(
                    stbg_build_rect(
                        cursor_x,
                        cursor_y + cursor_y_offset,
                        cursor_x + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_WIDTH),
                        cursor_y + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_HEIGHT)
                    ),
                    stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_COLOR)
                );
            }
        }

        if (needs_clipping)
            stbg__rc_pop_clipping_rect();
    }
}