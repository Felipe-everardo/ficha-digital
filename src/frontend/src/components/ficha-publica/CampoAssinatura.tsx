import { useEffect, useRef, useState } from 'react'

type CampoAssinaturaProps = {
  valor: string | null
  aoAlterar: (valor: string | null) => void
}

export function CampoAssinatura({ valor, aoAlterar }: CampoAssinaturaProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const desenhandoRef = useRef(false)
  const possuiTracosRef = useRef(Boolean(valor))
  const [possuiTracos, setPossuiTracos] = useState(Boolean(valor))

  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas) return

    const proporcao = window.devicePixelRatio || 1
    const largura = Math.max(canvas.clientWidth, 280)
    const altura = 180
    canvas.width = largura * proporcao
    canvas.height = altura * proporcao

    const contexto = canvas.getContext('2d')
    if (!contexto) return

    contexto.scale(proporcao, proporcao)
    contexto.lineCap = 'round'
    contexto.lineJoin = 'round'
    contexto.lineWidth = 2.4
    contexto.strokeStyle = '#273126'

    if (valor) {
      const imagem = new Image()
      imagem.onload = () => contexto.drawImage(imagem, 0, 0, largura, altura)
      imagem.src = valor
    }
  }, [valor])

  function obterPonto(event: React.PointerEvent<HTMLCanvasElement>) {
    const retangulo = event.currentTarget.getBoundingClientRect()
    return {
      x: event.clientX - retangulo.left,
      y: event.clientY - retangulo.top,
    }
  }

  function iniciar(event: React.PointerEvent<HTMLCanvasElement>) {
    const contexto = event.currentTarget.getContext('2d')
    if (!contexto) return

    const ponto = obterPonto(event)
    desenhandoRef.current = true
    event.currentTarget.setPointerCapture(event.pointerId)
    contexto.beginPath()
    contexto.moveTo(ponto.x, ponto.y)
  }

  function desenhar(event: React.PointerEvent<HTMLCanvasElement>) {
    if (!desenhandoRef.current) return

    const contexto = event.currentTarget.getContext('2d')
    if (!contexto) return

    const ponto = obterPonto(event)
    contexto.lineTo(ponto.x, ponto.y)
    contexto.stroke()
    possuiTracosRef.current = true
    setPossuiTracos(true)
  }

  function finalizar(event: React.PointerEvent<HTMLCanvasElement>) {
    if (!desenhandoRef.current) return

    desenhandoRef.current = false
    event.currentTarget.releasePointerCapture(event.pointerId)
    if (possuiTracosRef.current) {
      aoAlterar(event.currentTarget.toDataURL('image/png'))
    }
  }

  function limpar() {
    const canvas = canvasRef.current
    const contexto = canvas?.getContext('2d')
    if (canvas && contexto) {
      contexto.clearRect(0, 0, canvas.width, canvas.height)
    }

    setPossuiTracos(false)
    possuiTracosRef.current = false
    aoAlterar(null)
  }

  return (
    <div className="signature-field">
      <div className="signature-field__heading">
        <div>
          <span>Assinatura desenhada *</span>
          <small>Assine dentro do espaço usando o dedo ou o mouse.</small>
        </div>
        <button type="button" onClick={limpar} disabled={!possuiTracos}>
          Limpar
        </button>
      </div>
      <canvas
        ref={canvasRef}
        aria-label="Área para desenhar a assinatura"
        className="signature-canvas"
        onPointerDown={iniciar}
        onPointerMove={desenhar}
        onPointerUp={finalizar}
        onPointerCancel={finalizar}
      />
      <span className="signature-line" aria-hidden="true" />
    </div>
  )
}
