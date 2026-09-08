import type { InputHTMLAttributes } from 'react'
import { formatarEntradaMesAno } from '../utils/periodo'

type MonthInputProps = Omit<
  InputHTMLAttributes<HTMLInputElement>,
  'type' | 'value' | 'onChange'
> & {
  value: string
  onValueChange: (value: string) => void
}

export function MonthInput({
  value,
  onValueChange,
  placeholder = 'Ex.: 09/2026 ou 09',
  ...props
}: MonthInputProps) {
  return (
    <input
      {...props}
      type="text"
      inputMode="numeric"
      maxLength={7}
      pattern="(0?[1-9]|1[0-2])(/[0-9]{4})?"
      placeholder={placeholder}
      title="Informe o mês como MM/AAAA ou somente MM para usar o ano atual."
      value={value}
      onChange={(event) =>
        onValueChange(formatarEntradaMesAno(event.target.value))
      }
    />
  )
}
