import type { PreencherDadosPessoaisInput } from '../../services/api'

export type RespostasQuestionario = {
  temDiabetes: boolean | null
  tipoDiabetes: string
  possuiPressaoAlta: boolean | null
  temAlergia: boolean | null
  descricaoAlergia: string
  possuiCondicaoCardiaca: boolean | null
  temEpilepsia: boolean | null
  temHemofilia: boolean | null
  usaMarcaPasso: boolean | null
  estaGravidaOuAmamentando: boolean | null
}

export type DadosPessoaisFormulario = PreencherDadosPessoaisInput

export type CampoConfirmacao =
  | 'maioridade'
  | 'dadosPessoais'
  | 'questionarioSaude'
  | 'aceiteTermo'
