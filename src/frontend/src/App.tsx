import { useEffect, useState } from 'react'
import './App.css'
import { getApiStatus, type ApiStatus } from './services/api'

function App() {
  const [apiStatus, setApiStatus] = useState<ApiStatus | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getApiStatus()
      .then(setApiStatus)
      .catch(() => setError('Não foi possível acessar a API.'))
  }, [])

  const connectionState = error
    ? 'error'
    : apiStatus
      ? 'success'
      : 'loading'

  return (
    <main className="page-shell">
      <section className="hero-card" aria-labelledby="page-title">
        <div className="brand-mark" aria-hidden="true">
          FD
        </div>

        <p className="eyebrow">Estúdio de tatuagem</p>
        <h1 id="page-title">Ficha Digital</h1>
        <p className="intro">
          A base do sistema está pronta. Esta primeira tela verifica se o
          frontend React consegue conversar com a API em ASP.NET Core.
        </p>

        <div className={`status-card status-card--${connectionState}`}>
          <span className="status-dot" aria-hidden="true" />
          <div>
            <p className="status-label">Conexão com a API</p>
            <p className="status-message" aria-live="polite">
              {error ?? apiStatus?.message ?? 'Verificando a comunicação entre frontend e backend...'}
            </p>
            {apiStatus && (
              <p className="status-message"> Versão {apiStatus.version}
              </p>
            )}
          </div>
        </div>

        <div className="next-step">
          <span>Próxima etapa</span>
          <strong>Cadastro inicial do cliente</strong>
        </div>
      </section>
    </main>
  )
}

export default App
