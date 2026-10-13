import { AppRoutes } from './app/AppRoutes'
import './App.css'
import './MobileExperience.css'

function App() {
  return (
    <>
      {__FICHA_AMBIENTE__ === 'demo' && (
        <aside className="aviso-demonstracao" aria-label="Ambiente de demonstração">
          <strong>Demonstração</strong> — Use somente dados e assinaturas fictícios.
        </aside>
      )}
      <AppRoutes />
    </>
  )
}

export default App
