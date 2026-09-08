import {
  type InputHTMLAttributes,
  type MouseEvent,
  useRef,
} from 'react'
import './CalendarInput.css'

type CalendarInputProps = Omit<
  InputHTMLAttributes<HTMLInputElement>,
  'type'
> & {
  type: 'date' | 'month'
}

export function CalendarInput({
  onClick,
  ...props
}: CalendarInputProps) {
  const inputRef = useRef<HTMLInputElement>(null)

  function abrirCalendario(event: MouseEvent<HTMLInputElement>) {
    onClick?.(event)

    if (event.defaultPrevented || props.disabled || props.readOnly) {
      return
    }

    try {
      inputRef.current?.showPicker()
    } catch {
      inputRef.current?.focus()
    }
  }

  return (
    <span className="calendar-input">
      <input
        {...props}
        ref={inputRef}
        type={props.type}
        onClick={abrirCalendario}
      />
    </span>
  )
}
