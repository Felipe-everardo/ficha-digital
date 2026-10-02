import { QRCodeSVG } from 'qrcode.react'

export function ConviteQrCode({ link }: { link: string }) {
  return (
    <div className="invitation-qr">
      <QRCodeSVG
        value={link}
        size={256}
        level="M"
        marginSize={4}
        role="img"
        title="QR Code para preencher a ficha"
        aria-label="QR Code para preencher a ficha"
      />
      <p>
        O cliente pode apontar a câmera do celular para este código ou receber
        o link abaixo. As duas opções abrem a mesma ficha.
      </p>
      <p>
        O convite vale por 1 hora para preencher e confirmar a ficha, antes de
        iniciar o procedimento.
      </p>
    </div>
  )
}
