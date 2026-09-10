import { StudioBrand } from '../components/StudioBrand'
import { FormularioConsentimento } from '../components/ficha-publica/FormularioConsentimento'
import { FormularioDadosPessoais } from '../components/ficha-publica/FormularioDadosPessoais'
import { FormularioQuestionarioSaude } from '../components/ficha-publica/FormularioQuestionarioSaude'
import { PainelConclusao } from '../components/ficha-publica/PainelConclusao'
import { ProgressoFicha } from '../components/ficha-publica/ProgressoFicha'
import { ResumoProcedimento } from '../components/ficha-publica/ResumoProcedimento'
import { useFichaPublica } from '../hooks/useFichaPublica'
import './FichaPublicaPage.css'

export function FichaPublicaPage() {
  const fluxo = useFichaPublica()
  const { estado, dadosPessoais, questionario, consentimento } = fluxo

  return (
    <main className="public-page-shell">
      <section className="public-card" aria-labelledby="public-page-title">
        <header className="public-header">
          <StudioBrand compacta />

          <div>
            <p className="eyebrow">Área segura do cliente</p>
            <h1 id="public-page-title">Sua ficha digital</h1>
            <p className="intro">
              Preencha as informações com calma. Seus dados serão utilizados
              somente para o atendimento no estúdio.
            </p>
          </div>
        </header>

        <ProgressoFicha {...fluxo.progresso} />

        <div className="public-content" aria-live="polite">
          {estado.tipo === 'carregando' && (
            <div className="opening-state" aria-busy="true">
              <span className="loading-indicator" aria-hidden="true" />
              <div>
                <h2>Validando seu convite</h2>
                <p>Isso deve levar apenas alguns segundos.</p>
              </div>
            </div>
          )}

          {estado.tipo === 'sem-token' && (
            <div className="opening-state opening-state--warning" role="alert">
              <span className="state-symbol" aria-hidden="true">
                !
              </span>
              <div>
                <h2>Link incompleto</h2>
                <p>
                  Abra novamente o link completo enviado pelo estúdio. Nenhuma
                  informação foi enviada.
                </p>
              </div>
            </div>
          )}

          {estado.tipo === 'erro' && (
            <div className="opening-state opening-state--error" role="alert">
              <span className="state-symbol" aria-hidden="true">
                !
              </span>
              <div>
                <h2>{estado.titulo}</h2>
                <p>{estado.mensagem}</p>
                <button
                  className="secondary-button compact-button"
                  type="button"
                  onClick={fluxo.tentarNovamente}
                >
                  Tentar novamente
                </button>
              </div>
            </div>
          )}

          {estado.tipo === 'aberto' && !consentimento.termoAceito && (
            <ResumoProcedimento convite={estado.convite} />
          )}

          {estado.tipo === 'aberto' &&
            !dadosPessoais.preenchidos &&
            !consentimento.termoAceito && (
              <FormularioDadosPessoais
                convite={estado.convite}
                dados={dadosPessoais.valores}
                possuiDadosAnteriores={dadosPessoais.possuiDadosAnteriores}
                dataMaximaNascimento={dadosPessoais.dataMaximaNascimento}
                erro={dadosPessoais.erro}
                enviando={dadosPessoais.enviando}
                aoEnviar={dadosPessoais.enviar}
                aoAlterar={dadosPessoais.alterar}
              />
            )}

          {estado.tipo === 'aberto' &&
            dadosPessoais.preenchidos &&
            !questionario.respondido &&
            !consentimento.termoAceito && (
              <FormularioQuestionarioSaude
                respostas={questionario.respostas}
                erro={questionario.erro}
                enviando={questionario.enviando}
                aoEnviar={questionario.enviar}
                aoAlterar={questionario.alterar}
              />
            )}

          {estado.tipo === 'aberto' &&
            questionario.respondido &&
            !consentimento.termoAceito && (
              <FormularioConsentimento
                convite={estado.convite}
                dados={dadosPessoais.valores}
                respostas={questionario.respostas}
                confirmacoes={consentimento.confirmacoes}
                nomeAssinante={consentimento.nomeAssinante}
                erro={consentimento.erro}
                enviando={consentimento.enviando}
                aoEnviar={consentimento.enviar}
                aoAlterarConfirmacao={consentimento.alterarConfirmacao}
                aoAlterarNomeAssinante={consentimento.alterarNomeAssinante}
              />
            )}

          {consentimento.termoAceito && (
            <PainelConclusao termoAceito={consentimento.termoAceito} />
          )}
        </div>
      </section>
    </main>
  )
}
