import logoManuscrito from '../assets/manuscrito-estudio-logo.png'
import './StudioBrand.css'

type StudioBrandProps = {
  compacta?: boolean
  contexto?: string
  className?: string
}

export function StudioBrand({
  compacta = false,
  contexto,
  className = '',
}: StudioBrandProps) {
  const classes = [
    'studio-brand',
    compacta ? 'studio-brand--compact' : '',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <div className={classes}>
      <span className="studio-brand__logo-frame">
        <img
          className="studio-brand__logo"
          src={logoManuscrito}
          alt={compacta ? 'Manuscrito Estudio' : ''}
        />
      </span>

      {!compacta && (
        <span className="studio-brand__copy">
          <strong>Manuscrito Estudio</strong>
          {contexto && <small>{contexto}</small>}
        </span>
      )}
    </div>
  )
}
