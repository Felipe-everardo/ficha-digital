type ProgressoFichaProps = {
  conviteAberto: boolean
  dadosPessoaisConcluidos: boolean
  questionarioConcluido: boolean
  fichaConcluida: boolean
}

export function ProgressoFicha({
  conviteAberto,
  dadosPessoaisConcluidos,
  questionarioConcluido,
  fichaConcluida,
}: ProgressoFichaProps) {
  return (
    <ol className="flow-progress" aria-label="Etapas da ficha">
      <li
        className={`flow-progress__item ${
          conviteAberto
            ? 'flow-progress__item--complete'
            : 'flow-progress__item--active'
        }`}
      >
        <span>{conviteAberto ? '✓' : '1'}</span>
        Validar convite
      </li>
      <li
        className={`flow-progress__item ${
          dadosPessoaisConcluidos
            ? 'flow-progress__item--complete'
            : conviteAberto
              ? 'flow-progress__item--active'
              : ''
        }`}
      >
        <span>{dadosPessoaisConcluidos ? '✓' : '2'}</span>
        Dados pessoais
      </li>
      <li
        className={`flow-progress__item ${
          questionarioConcluido
            ? 'flow-progress__item--complete'
            : dadosPessoaisConcluidos
              ? 'flow-progress__item--active'
              : ''
        }`}
      >
        <span>{questionarioConcluido ? '✓' : '3'}</span>
        Saúde
      </li>
      <li
        className={`flow-progress__item ${
          fichaConcluida
            ? 'flow-progress__item--complete'
            : questionarioConcluido
              ? 'flow-progress__item--active'
              : ''
        }`}
      >
        <span>{fichaConcluida ? '✓' : '4'}</span>
        Consentimento
      </li>
    </ol>
  )
}
