import type { FormEventHandler } from 'react'
import type { ConviteFichaAberto } from '../../services/api'
import { formatarCep, formatarCpf } from '../../utils/documentos'
import { formatarTelefoneBrasileiro } from '../../utils/telefone'
import { CalendarInput } from '../CalendarInput'
import type { DadosPessoaisFormulario } from './model'

type FormularioDadosPessoaisProps = {
  convite: ConviteFichaAberto
  dados: DadosPessoaisFormulario
  possuiDadosAnteriores: boolean
  dataMaximaNascimento: string
  erro: string | null
  enviando: boolean
  aoEnviar: FormEventHandler<HTMLFormElement>
  aoAlterar: (campo: keyof DadosPessoaisFormulario, valor: string) => void
}

export function FormularioDadosPessoais({
  convite,
  dados,
  possuiDadosAnteriores,
  dataMaximaNascimento,
  erro,
  enviando,
  aoEnviar,
  aoAlterar,
}: FormularioDadosPessoaisProps) {
  return (
    <form className="personal-data-form" onSubmit={aoEnviar}>
      <div className="section-heading">
        <p className="eyebrow">Etapa 2 de 4</p>
        <h2>
          {possuiDadosAnteriores
            ? 'Confira seus dados pessoais'
            : 'Seus dados pessoais'}
        </h2>
        {possuiDadosAnteriores ? (
          <p>
            Encontramos os dados do seu atendimento anterior. Confira todos os
            campos e atualize o que mudou antes de continuar.
          </p>
        ) : (
          <p>
            Este convite foi criado para <strong>{convite.nomeReferencia}</strong>.
            Complete seus dados para continuar para o histórico de saúde.
          </p>
        )}
      </div>

      <div className="personal-data-grid">
        <label className="personal-field personal-field--full">
          <span>Nome completo *</span>
          <input
            type="text"
            autoComplete="name"
            maxLength={150}
            placeholder="Ex.: Maria da Silva"
            required
            value={dados.nomeCompleto}
            onChange={(event) => aoAlterar('nomeCompleto', event.target.value)}
          />
        </label>

        <label className="personal-field">
          <span>Nome social (opcional)</span>
          <input
            type="text"
            autoComplete="nickname"
            maxLength={150}
            placeholder="Ex.: Mari"
            value={dados.nomeSocial}
            onChange={(event) => aoAlterar('nomeSocial', event.target.value)}
          />
        </label>

        <label className="personal-field">
          <span>Pronomes (opcional)</span>
          <input
            type="text"
            maxLength={50}
            placeholder="Ex.: ela/dela"
            value={dados.pronomes}
            onChange={(event) => aoAlterar('pronomes', event.target.value)}
          />
        </label>

        <label className="personal-field">
          <span>Estado civil *</span>
          <input
            type="text"
            maxLength={50}
            placeholder="Ex.: Solteira"
            required
            value={dados.estadoCivil}
            onChange={(event) => aoAlterar('estadoCivil', event.target.value)}
          />
        </label>

        <label className="personal-field">
          <span>Data de nascimento *</span>
          <CalendarInput
            autoComplete="bday"
            max={dataMaximaNascimento}
            required
            value={dados.dataNascimento}
            onValueChange={(valor) => aoAlterar('dataNascimento', valor)}
          />
          <small>Atendimento exclusivo para maiores de 18 anos.</small>
        </label>

        <label className="personal-field">
          <span>CPF *</span>
          <input
            type="text"
            autoComplete="off"
            maxLength={14}
            inputMode="numeric"
            placeholder="000.000.000-00"
            required
            value={dados.cpf}
            onChange={(event) =>
              aoAlterar('cpf', formatarCpf(event.target.value))
            }
          />
        </label>

        <label className="personal-field">
          <span>Celular / WhatsApp *</span>
          <input
            type="tel"
            autoComplete="tel"
            maxLength={15}
            inputMode="numeric"
            placeholder="Ex.: (21) 99999-9999"
            required
            value={dados.celular}
            onChange={(event) =>
              aoAlterar(
                'celular',
                formatarTelefoneBrasileiro(event.target.value),
              )
            }
          />
        </label>

        <label className="personal-field">
          <span>E-mail (opcional)</span>
          <input
            type="email"
            autoComplete="email"
            maxLength={254}
            placeholder="Ex.: maria@email.com"
            value={dados.email}
            onChange={(event) => aoAlterar('email', event.target.value)}
          />
        </label>

        <label className="personal-field">
          <span>Telefone adicional (opcional)</span>
          <input
            type="tel"
            maxLength={15}
            inputMode="numeric"
            placeholder="Ex.: (21) 3333-3333"
            value={dados.telefoneAdicional}
            onChange={(event) =>
              aoAlterar(
                'telefoneAdicional',
                formatarTelefoneBrasileiro(event.target.value),
              )
            }
          />
        </label>

        <label className="personal-field">
          <span>Instagram (opcional)</span>
          <input
            type="text"
            maxLength={100}
            placeholder="@usuario"
            value={dados.instagram}
            onChange={(event) => aoAlterar('instagram', event.target.value)}
          />
        </label>

        <div className="personal-field-group personal-field--full">
          <div>
            <h3>Contato de emergência (opcional)</h3>
            <p>Se preencher, informe o nome e o celular.</p>
          </div>
          <div className="personal-data-grid">
            <label className="personal-field">
              <span>Nome do contato</span>
              <input
                type="text"
                maxLength={150}
                placeholder="Ex.: João da Silva"
                value={dados.contatoEmergenciaNome}
                onChange={(event) =>
                  aoAlterar('contatoEmergenciaNome', event.target.value)
                }
              />
            </label>
            <label className="personal-field">
              <span>Celular do contato</span>
              <input
                type="tel"
                maxLength={15}
                inputMode="numeric"
                placeholder="Ex.: (21) 98888-8888"
                value={dados.contatoEmergenciaCelular}
                onChange={(event) =>
                  aoAlterar(
                    'contatoEmergenciaCelular',
                    formatarTelefoneBrasileiro(event.target.value),
                  )
                }
              />
            </label>
          </div>
        </div>

        <div className="personal-field-group personal-field--full">
          <div>
            <h3>Endereço</h3>
            <p>Informe seu endereço residencial atual.</p>
          </div>
          <div className="personal-data-grid">
            <label className="personal-field">
              <span>CEP *</span>
              <input
                type="text"
                autoComplete="postal-code"
                maxLength={9}
                inputMode="numeric"
                placeholder="00000-000"
                required
                value={dados.cep}
                onChange={(event) =>
                  aoAlterar('cep', formatarCep(event.target.value))
                }
              />
            </label>

            <label className="personal-field">
              <span>Logradouro *</span>
              <input
                type="text"
                autoComplete="street-address"
                maxLength={150}
                placeholder="Rua, avenida ou travessa"
                required
                value={dados.logradouro}
                onChange={(event) =>
                  aoAlterar('logradouro', event.target.value)
                }
              />
            </label>

            <label className="personal-field">
              <span>Número *</span>
              <input
                type="text"
                maxLength={20}
                placeholder="Ex.: 123 ou S/N"
                required
                value={dados.numero}
                onChange={(event) => aoAlterar('numero', event.target.value)}
              />
            </label>

            <label className="personal-field">
              <span>Complemento (opcional)</span>
              <input
                type="text"
                maxLength={100}
                placeholder="Ex.: Apto. 201"
                value={dados.complemento}
                onChange={(event) =>
                  aoAlterar('complemento', event.target.value)
                }
              />
            </label>

            <label className="personal-field">
              <span>Bairro *</span>
              <input
                type="text"
                maxLength={100}
                required
                value={dados.bairro}
                onChange={(event) => aoAlterar('bairro', event.target.value)}
              />
            </label>

            <label className="personal-field">
              <span>Cidade *</span>
              <input
                type="text"
                autoComplete="address-level2"
                maxLength={100}
                required
                value={dados.cidade}
                onChange={(event) => aoAlterar('cidade', event.target.value)}
              />
            </label>

            <label className="personal-field">
              <span>Estado (UF) *</span>
              <input
                type="text"
                autoComplete="address-level1"
                maxLength={2}
                placeholder="RJ"
                required
                value={dados.estado}
                onChange={(event) =>
                  aoAlterar('estado', event.target.value.toUpperCase())
                }
              />
            </label>
          </div>
        </div>
      </div>

      {erro && (
        <p className="form-error" role="alert">
          {erro}
        </p>
      )}

      <div className="questionnaire-actions">
        <p>
          {possuiDadosAnteriores
            ? 'Ao continuar, você confirma que estes dados estão atualizados.'
            : 'Confira as informações antes de continuar. Elas ficarão associadas ao seu histórico no estúdio.'}
        </p>
        <button type="submit" disabled={enviando}>
          {enviando
            ? 'Salvando dados...'
            : possuiDadosAnteriores
              ? 'Confirmar dados e continuar'
              : 'Salvar e continuar'}
        </button>
      </div>
    </form>
  )
}
