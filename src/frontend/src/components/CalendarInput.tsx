import {
  type ChangeEvent,
  type InputHTMLAttributes,
  useEffect,
  useState,
} from 'react'
import './CalendarInput.css'

type CalendarInputProps = Omit<
  InputHTMLAttributes<HTMLInputElement>,
  'type' | 'value' | 'onChange'
> & {
  value: string
  onValueChange: (value: string) => void
}

function formatarIso(value: string) {
  if (!value) return ''
  const [ano, mes, dia] = value.split('-')
  return ano && mes && dia ? `${dia}/${mes}/${ano}` : value
}

function aplicarMascara(value: string) {
  const digitos = value.replace(/\D/g, '').slice(0, 8)
  if (digitos.length <= 2) return digitos
  if (digitos.length <= 4) {
    return `${digitos.slice(0, 2)}/${digitos.slice(2)}`
  }
  return `${digitos.slice(0, 2)}/${digitos.slice(2, 4)}/${digitos.slice(4)}`
}

function converterParaIso(value: string) {
  const correspondencia = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(value)
  if (!correspondencia) return null

  const [, dia, mes, ano] = correspondencia
  const iso = `${ano}-${mes}-${dia}`
  const data = new Date(`${iso}T00:00:00Z`)
  return data.getUTCFullYear() === Number(ano) &&
    data.getUTCMonth() + 1 === Number(mes) &&
    data.getUTCDate() === Number(dia)
    ? iso
    : null
}

export function CalendarInput({
  value,
  onValueChange,
  min,
  max,
  ...props
}: CalendarInputProps) {
  const [texto, setTexto] = useState(() => formatarIso(value))

  useEffect(() => {
    setTexto(formatarIso(value))
  }, [value])

  function alterar(event: ChangeEvent<HTMLInputElement>) {
    const novoTexto = aplicarMascara(event.target.value)
    const iso = converterParaIso(novoTexto)
    let mensagem = ''

    if (novoTexto && !iso) {
      mensagem = novoTexto.length < 10
        ? 'Informe a data completa no formato DD/MM/AAAA.'
        : 'Informe uma data válida.'
    } else if (iso && typeof min === 'string' && iso < min) {
      mensagem = `A data deve ser igual ou posterior a ${formatarIso(min)}.`
    } else if (iso && typeof max === 'string' && iso > max) {
      mensagem = `A data deve ser igual ou anterior a ${formatarIso(max)}.`
    }

    event.target.setCustomValidity(mensagem)
    setTexto(novoTexto)

    if (!novoTexto) onValueChange('')
    else if (iso && !mensagem) onValueChange(iso)
  }

  return (
    <span className="calendar-input">
      <input
        {...props}
        type="text"
        inputMode="numeric"
        placeholder="DD/MM/AAAA"
        maxLength={10}
        value={texto}
        onChange={alterar}
      />
    </span>
  )
}
