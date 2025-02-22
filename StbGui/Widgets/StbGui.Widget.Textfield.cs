#pragma warning disable IDE1006 // Naming Styles

namespace StbSharp;

using System;
using widget_id = int;
using widget_hash = int;
using font_id = int;

public partial class StbGui
{
    private static void stbg__textfield_init_default_theme()
    {
        var font_style = context.theme.default_font_style;

        // TEXTFIELD
        var textfieldPaddingTopBottom = MathF.Ceiling(font_style.size / 4);
        var textfieldPaddingLeftRight = MathF.Ceiling(font_style.size / 4);

        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_TOP, textfieldPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_BOTTOM, textfieldPaddingTopBottom);
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_LEFT, textfieldPaddingLeftRight);
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_RIGHT, textfieldPaddingLeftRight);

        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_BACKGROUND_COLOR, rgb(236, 240, 241));
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_BORDER_COLOR, rgb(52, 73, 94));
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_BORDER_SIZE, 1);
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_TEXT_COLOR, rgb(44, 62, 80));
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_CURSOR_COLOR, rgb(44, 62, 80));
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_CURSOR_WIDTH, 1);
        stbg_set_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_CURSOR_HEIGHT, font_style.size);
    }

    private static ref stbg_widget stbg__textfield_create(ReadOnlySpan<char> identifier, Memory<char> text, ref int text_length)
    {
        ref var textfield = ref stbg__add_widget(STBG_WIDGET_TYPE.TEXTFIELD, identifier, out var is_new);

        textfield.properties.text_editable = text;

        ref var layout = ref textfield.properties.layout;

        layout.constrains = stbg_build_constrains_unconstrained();
        layout.constrains.min.height = context.theme.default_font_style.size + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_TOP, STBG_WIDGET_STYLE.TEXTFIELD_PADDING_BOTTOM) - 3;
        layout.constrains.min.width = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_LEFT, STBG_WIDGET_STYLE.TEXTFIELD_PADDING_RIGHT);
        layout.inner_padding = new stbg_padding()
        {
            top = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_TOP),
            bottom = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_BOTTOM),
            left = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_LEFT),
            right = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_RIGHT),
        };
        layout.intrinsic_size = stbg__build_intrinsic_size_pixels(300, layout.constrains.min.height);

        if (is_new || (textfield.properties.input_flags & STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED) == 0)
        {
            textfield.properties.text_editable_length = text_length;
        }
        else
        {
            text_length = textfield.properties.text_editable_length;
            textfield.properties.input_flags &= ~STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
        }

        return ref textfield;
    }

    private static bool stbg__textfield_update_input(ref stbg_widget textfield)
    {
        if (context.input_feedback.hovered_widget_id == textfield.id && context.input.mouse_button_1_down)
        {
            context.input_feedback.editing_text_widget_id = textfield.id;
        }

        if (context.input_feedback.editing_text_widget_id == textfield.id)
        {
            ref var str = ref context.text_edit.str;
            ref var state = ref context.text_edit.state;

            if (context.text_edit.widget_id != textfield.id || context.text_edit.widget_hash != textfield.hash)
            {
                context.text_edit.widget_id = textfield.id;
                context.text_edit.widget_hash = textfield.hash;

                str = new StbTextEdit.STB_TEXTEDIT_STRING()
                {
                    text = textfield.properties.text_editable,
                    text_length = textfield.properties.text_editable_length,
                    get_width = (str, n, i) =>
                    {
                        if (i == 1 && str.text.Span[n] == '\n')
                            return StbTextEdit.STB_TEXTEDIT_GETWIDTH_NEWLINE;

                        // TODO: this can be optimized (we want the X difference between the last two consecutive characters)

                        var w1 = stbg__measure_text(stbg__build_text(str.text.Slice(n, i))).width;
                        var w2 = stbg__measure_text(stbg__build_text(str.text.Slice(n, i + 1))).width;
                        return (int)(w2 - w1);
                    },
                    layout_row = (str, n) =>
                    {
                        var row = new StbTextEdit.StbTexteditRow();

                        var text = str.text.Slice(0, str.text_length).Slice(n);

                        //TODO: Handle word-wrapping!!
                        var size = stbg__measure_text(stbg__build_text(text));

                        row.x0 = 0;
                        row.x1 = size.width;
                        row.ymin = 0;
                        row.ymax = size.height;
                        row.num_chars = text.Length;

                        return row;
                    }

                };

                StbTextEdit.stb_textedit_initialize_state(ref state, true);
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
                            if (!stbg_rect_is_position_inside(textfield.properties.computed_bounds.global_rect, context.input.mouse_position))
                            {
                                stop_editing = true;
                                break;
                            }

                            var textfield_local_position = stbg_offset_position(context.input.mouse_position, -textfield.properties.computed_bounds.global_rect.x0 - stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_LEFT), -textfield.properties.computed_bounds.global_rect.y0 - stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_TOP));
                            StbTextEdit.stb_textedit_click(ref str, ref state, textfield_local_position.x, textfield_local_position.y);
                        }
                        break;

                    case STBG_INPUT_EVENT_TYPE.MOUSE_POSITION:
                        if (context.input.mouse_button_1)
                        {
                            var textfield_local_position = stbg_offset_position(context.input.mouse_position, -textfield.properties.computed_bounds.global_rect.x0 - stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_LEFT), -textfield.properties.computed_bounds.global_rect.y0 - stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_TOP));
                            StbTextEdit.stb_textedit_drag(ref str, ref state, textfield_local_position.x, textfield_local_position.y);
                        }
                        break;

                    case STBG_INPUT_EVENT_TYPE.KEYBOARD_KEY:
                        {
                            int textedit_key = 0;

                            if (user_event.key_pressed)
                            {
                                switch (user_event.key)
                                {
                                    case STBG_KEYBOARD_KEY.CHARACTER:
                                        textedit_key = user_event.key_character;
                                        break;
                                    case STBG_KEYBOARD_KEY.LEFT:
                                        textedit_key = StbTextEdit.STB_TEXTEDIT_K_LEFT;
                                        break;
                                    case STBG_KEYBOARD_KEY.RIGHT:
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
                                }

                                if ((user_event.key_modifiers & STBG_KEYBOARD_MODIFIER_FLAGS.SHIFT) != 0)
                                    textedit_key |= StbTextEdit.STB_TEXTEDIT_K_SHIFT;

                                StbTextEdit.stb_textedit_key(ref str, ref state, textedit_key);

                                textfield.properties.text_editable_length = str.text_length;

                                textfield.properties.input_flags |= STBG_WIDGET_INPUT_FLAGS.VALUE_UPDATED;
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
            }
        }

        return false;
    }

    private static void stbg__textfield_render(ref stbg_widget textfield)
    {
        var size = textfield.properties.computed_bounds.size;

        stbg__rc_draw_border(
            stbg_build_rect(0, 0, size.width, size.height),
            stbg_get_widget_style(STBG_WIDGET_STYLE.TEXTFIELD_BORDER_SIZE),
            stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTFIELD_BORDER_COLOR),
            stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTFIELD_BACKGROUND_COLOR)
        );

        var text = textfield.properties.text_editable.Slice(0, textfield.properties.text_editable_length);

        stbg__rc_draw_text(
            stbg_build_rect(
                stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_LEFT),
                stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_TOP),
                size.width - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_RIGHT),
                size.height - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_BOTTOM)
            ),
            stbg__build_text(text, stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTFIELD_TEXT_COLOR))
        );

        if (context.input_feedback.editing_text_widget_id == textfield.id)
        {
            // TODO: This sucks! we are measuring the whole text only to get a simple cursor position?
            var cursor_position = stbg__measure_text(stbg__build_text(text.Slice(0, Math.Min(context.text_edit.state.cursor, text.Length))));

            var cursor_x = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_LEFT) + cursor_position.width;
            var cursor_y = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_TOP);

            stbg__rc_draw_rectangle(
                stbg_build_rect(
                    cursor_x,
                    cursor_y,
                    cursor_x + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_CURSOR_WIDTH),
                    cursor_y + stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_CURSOR_HEIGHT)
                ),
                stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTFIELD_CURSOR_COLOR)
            );

            if (context.text_edit.state.select_start != context.text_edit.state.select_end)
            {
                var select_start = Math.Clamp(Math.Min(context.text_edit.state.select_start, context.text_edit.state.select_end), 0, text.Length);
                var select_end = Math.Clamp(Math.Max(context.text_edit.state.select_start, context.text_edit.state.select_end), 0, text.Length);

                // Draw selection background
                var select_start_position = stbg__measure_text(stbg__build_text(text.Slice(0, select_start)));
                var select_end_position = stbg__measure_text(stbg__build_text(text.Slice(0, select_end)));

                var select_start_position_x = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_LEFT) + select_start_position.width;
                var select_end_position_x = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_LEFT) + select_end_position.width;
                var select_y = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_TOP);
                var select_y_end = stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_TOP) + Math.Max(select_start_position.height, select_start_position.height);

                stbg__rc_draw_rectangle(
                    stbg_build_rect(
                        select_start_position_x,
                        select_y,
                        select_end_position_x,
                        select_y_end
                    ),
                    stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTFIELD_CURSOR_COLOR)
                );

                // Draw text in inverted color over the selection background
                var selected_text = text.Slice(select_start, select_end - select_start);
                stbg__rc_draw_text(
                    stbg_build_rect(
                        select_start_position_x,
                        select_y,
                        size.width - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_RIGHT),
                        size.height - stbg__sum_styles(STBG_WIDGET_STYLE.TEXTFIELD_PADDING_BOTTOM)
                    ),
                    stbg__build_text(selected_text, stbg_get_widget_style_color(STBG_WIDGET_STYLE.TEXTFIELD_BACKGROUND_COLOR))
                );
            }
        }
    }
}