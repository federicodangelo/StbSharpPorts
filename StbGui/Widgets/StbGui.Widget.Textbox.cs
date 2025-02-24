#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

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
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_HEIGHT, font_style.size);
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

    private static ref stbg_widget stbg__textbox_create(ReadOnlySpan<char> identifier, Memory<char> text, ref int text_length, bool single_line)
    {
        ref var textbox = ref stbg__add_widget(STBG_WIDGET_TYPE.TEXTBOX, identifier, out var is_new);

        var parameters = new stbg__textbox_parameters()
        {
            single_line = single_line
        };

        textbox.properties.text_editable = text;

        stbg__textbox_set_parameters(ref textbox, parameters);

        ref var layout = ref textbox.properties.layout;

        layout.constrains = stbg_build_constrains_unconstrained();
        layout.constrains.min.height = context.theme.default_font_style.size * (single_line ? 1 : 2) + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP, STBG_WIDGET_STYLE.TEXTBOX_PADDING_BOTTOM);
        layout.constrains.min.width = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT, STBG_WIDGET_STYLE.TEXTBOX_PADDING_RIGHT);
        layout.inner_padding = new stbg_padding()
        {
            top = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP),
            bottom = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_BOTTOM),
            left = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT),
            right = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_RIGHT),
        };
        layout.intrinsic_size = stbg__build_intrinsic_size_pixels(300, layout.constrains.min.height);

        if (is_new || (textbox.properties.input_flags & STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED) == 0)
        {
            textbox.properties.text_editable_length = text_length;
        }
        else
        {
            text_length = textbox.properties.text_editable_length;
            textbox.properties.input_flags &= ~STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
        }

        return ref textbox;
    }

    private static bool stbg__textbox_update_input(ref stbg_widget textbox)
    {
        if (context.input_feedback.hovered_widget_id == textbox.id && context.input.mouse_button_1_down)
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

        if (context.input_feedback.editing_text_widget_id == textbox.id)
        {
            ref var str = ref context.text_edit.str;
            ref var state = ref context.text_edit.state;

            if (context.text_edit.widget_id != textbox.id || context.text_edit.widget_hash != textbox.hash)
            {
                context.text_edit.widget_id = textbox.id;
                context.text_edit.widget_hash = textbox.hash;
                stbg__textbox_get_parameters(ref textbox, out var parameters);

                str = new StbTextEdit.STB_TEXTEDIT_STRING()
                {
                    text = textbox.properties.text_editable,
                    text_length = textbox.properties.text_editable_length,
                    get_width = (str, n, i) =>
                    {
                        if (i == 1 && str.text.Span[n] == '\n')
                            return StbTextEdit.STB_TEXTEDIT_GETWIDTH_NEWLINE;

                        // TODO: this can be optimized (we want the X difference between the last two consecutive characters)

                        var w1 = stbg__measure_text(stbg__build_text(str.text.Slice(n, i)), parameters.single_line ? STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE : STBG_MEASURE_TEXT_OPTIONS.NONE).width;
                        var w2 = stbg__measure_text(stbg__build_text(str.text.Slice(n, i + 1)), parameters.single_line ? STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE : STBG_MEASURE_TEXT_OPTIONS.NONE).width;
                        return (int)(w2 - w1);
                    },
                    layout_row = (str, n) =>
                    {
                        var row = new StbTextEdit.StbTexteditRow();

                        var text = str.text.Slice(0, str.text_length).Slice(n);

                        //TODO: Handle word-wrapping!!
                        var size = stbg__measure_text(stbg__build_text(text), parameters.single_line ? STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE : STBG_MEASURE_TEXT_OPTIONS.NONE);

                        row.x0 = 0;
                        row.x1 = size.width;
                        row.ymin = 0;
                        row.ymax = size.height;
                        row.num_chars = text.Length;

                        return row;
                    }

                };

                StbTextEdit.stb_textedit_initialize_state(ref state, parameters.single_line);
            }

            var stop_editing = false;

            for (int i = 0; i < context.user_input_events_queue_offset; i++)
            {
                var user_event = context.user_input_events_queue[i];

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

                            var textbox_local_position = stbg_offset_position(context.input.mouse_position, -textbox.properties.computed_bounds.global_rect.x0 - stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT), -textbox.properties.computed_bounds.global_rect.y0 - stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP));
                            StbTextEdit.stb_textedit_click(ref str, ref state, textbox_local_position.x, textbox_local_position.y);
                        }
                        break;

                    case STBG_INPUT_EVENT_TYPE.MOUSE_POSITION:
                        if (context.input.mouse_button_1)
                        {
                            var textbox_local_position = stbg_offset_position(context.input.mouse_position, -textbox.properties.computed_bounds.global_rect.x0 - stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT), -textbox.properties.computed_bounds.global_rect.y0 - stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP));
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
                                            if (control && c == 'z')
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
                                        break;
                                    case STBG_KEYBOARD_KEY.RIGHT:
                                        if (control)
                                            textedit_key = StbTextEdit.STB_TEXTEDIT_K_WORDRIGHT;
                                        else
                                            textedit_key = StbTextEdit.STB_TEXTEDIT_K_RIGHT;
                                        break;
                                    case STBG_KEYBOARD_KEY.UP:
                                        textedit_key = StbTextEdit.STB_TEXTEDIT_K_UP;
                                        break;
                                    case STBG_KEYBOARD_KEY.DOWN:
                                        textedit_key = StbTextEdit.STB_TEXTEDIT_K_DOWN;
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
                                    textbox.properties.text_editable_length = str.text_length;
                                    textbox.properties.input_flags |= STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
                                }
                            }
                            break;
                        }
                }
            }

            if (stop_editing)
            {
                context.input_feedback.editing_text_widget_id = STBG_WIDGET_ID_NULL;
                context.text_edit.widget_id = STBG_WIDGET_ID_NULL;
                context.text_edit.widget_hash = 0;

                if (context.input_feedback.ime_info.widget_id == textbox.id)
                    context.input_feedback.ime_info = new stbg_input_method_editor_info();
            }
        }

        return false;
    }

    private static void stbg__textbox_render(ref stbg_widget textbox)
    {
        var size = textbox.properties.computed_bounds.size;
        var editing = context.input_feedback.editing_text_widget_id == textbox.id;
        var text = textbox.properties.text_editable.Slice(0, textbox.properties.text_editable_length);
        stbg__textbox_get_parameters(ref textbox, out var parameters);

        var needs_clipping = editing;

        var text_render_options = needs_clipping ? STBG_RENDER_TEXT_OPTIONS.DONT_CLIP : STBG_RENDER_TEXT_OPTIONS.NONE; // No need to use clipping IF we are already clipping the whole textbox
        var text_measure_options = parameters.single_line ? STBG_MEASURE_TEXT_OPTIONS.SINGLE_LINE : STBG_MEASURE_TEXT_OPTIONS.NONE;

        float cursor_x, cursor_y;

        // We re-used the children offset values since a textbox can't have children..
        float draw_offset_x = editing ? textbox.properties.layout.children_offset.x : 0;
        float draw_offset_y = editing ? textbox.properties.layout.children_offset.y : 0;

        if (editing)
        {
            var cursor_position = stbg__get_character_position_in_text(stbg__build_text(text.Slice(0, text.Length)), context.text_edit.state.cursor, text_measure_options);

            cursor_x = draw_offset_x + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT) + cursor_position.x;
            cursor_y = draw_offset_y + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP);

            if (cursor_x >= size.width - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_RIGHT))
            {
                draw_offset_x = size.width - cursor_position.x - context.theme.default_font_style.size;
                cursor_x = draw_offset_x + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT) + cursor_position.x;
                textbox.properties.layout.children_offset.x = draw_offset_x;
            }
            else if (cursor_x < 0)
            {
                draw_offset_x = -cursor_position.x;
                cursor_x = draw_offset_x + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT) + cursor_position.x;
                textbox.properties.layout.children_offset.x = draw_offset_x;
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

        if (editing)
        {
            // Draw cursor
            stbg__rc_draw_rectangle(
                stbg_build_rect(
                    cursor_x,
                    cursor_y - 1,
                    cursor_x + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_WIDTH),
                    cursor_y + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_HEIGHT) + 1
                ),
                stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_COLOR)
            );

            // Draw selected text background
            if (context.text_edit.state.select_start != context.text_edit.state.select_end)
            {
                var select_start = Math.Clamp(Math.Min(context.text_edit.state.select_start, context.text_edit.state.select_end), 0, text.Length);
                var select_end = Math.Clamp(Math.Max(context.text_edit.state.select_start, context.text_edit.state.select_end), 0, text.Length);

                // Draw selection background
                var select_start_position = stbg__measure_text(stbg__build_text(text.Slice(0, select_start)), text_measure_options);
                var select_end_position = stbg__measure_text(stbg__build_text(text.Slice(0, select_end)), text_measure_options);

                var select_start_position_x = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT) + select_start_position.width;
                var select_end_position_x = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_LEFT) + select_end_position.width;
                var select_y = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP);
                var select_y_end = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_TOP) + Math.Max(select_start_position.height, select_start_position.height);

                stbg__rc_draw_rectangle(
                    stbg_build_rect(
                        draw_offset_x + select_start_position_x,
                        draw_offset_y + select_y - 1,
                        draw_offset_x + select_end_position_x,
                        draw_offset_y + select_y_end + 1
                    ),
                    stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTBOX_CURSOR_COLOR)
                );

                // Draw text in inverted color over the selection background
                var selected_text = text.Slice(select_start, select_end - select_start);
                stbg__rc_draw_text(
                    stbg_build_rect(
                        draw_offset_x + select_start_position_x,
                        draw_offset_y + select_y,
                        size.width - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_RIGHT),
                        size.height - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTBOX_PADDING_BOTTOM)
                    ),
                    stbg__build_text(selected_text, stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTBOX_BACKGROUND_COLOR)),
                    -1, -1,
                    text_measure_options,
                    text_render_options
                );
            }
        }

        if (needs_clipping)
            stbg__rc_pop_clipping_rect();
    }
}