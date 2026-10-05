import { spawn, spawnSync, type ChildProcess } from 'node:child_process'
import { mkdtempSync, rmSync } from 'node:fs'
import { tmpdir } from 'node:os'
import { createServer } from 'node:net'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const diretorioFrontend = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  '..',
)

export async function verificarPortaLivre(porta: number) {
  const server = createServer()
  await new Promise<void>((resolve, reject) => {
    server.once('error', () => reject(new Error(
      `A porta ${porta} está ocupada ou indisponível. Encerre os servidores locais antes do E2E.`,
    )))
    server.listen(porta, '127.0.0.1', () => {
      server.close(error => error ? reject(error) : resolve())
    })
  })
}

async function aguardarUrl(url: string, processo: ChildProcess) {
  const limite = Date.now() + 90_000

  while (Date.now() < limite) {
    if (processo.exitCode !== null || processo.signalCode !== null) {
      throw new Error(`O servidor E2E encerrou antes de responder em ${url}.`)
    }
    try {
      const response = await fetch(url, { signal: AbortSignal.timeout(2_000) })
      if (response.ok) return
    } catch {
      // O servidor ainda está iniciando.
    }

    await new Promise((resolve) => setTimeout(resolve, 300))
  }

  throw new Error(`O servidor E2E não respondeu em ${url}.`)
}

function iniciar(
  comando: string,
  argumentos: string[],
  opcoes: { cwd: string; env?: NodeJS.ProcessEnv },
) {
  const processo = spawn(comando, argumentos, {
    cwd: opcoes.cwd,
    env: opcoes.env ?? process.env,
    detached: process.platform !== 'win32',
    stdio: 'ignore',
    windowsHide: true,
  })
  return processo
}

async function encerrar(processo: ChildProcess) {
  if (!processo.pid || processo.exitCode !== null || processo.signalCode !== null) return

  const encerrou = new Promise<void>((resolve) => {
    processo.once('exit', () => resolve())
  })

  if (process.platform === 'win32') {
    processo.kill('SIGKILL')
    await Promise.race([
      encerrou,
      new Promise((resolve) => setTimeout(resolve, 2_000)),
    ])

    if (processo.exitCode === null && processo.signalCode === null) {
      spawnSync(
        'taskkill',
        ['/PID', String(processo.pid), '/T', '/F'],
        { stdio: 'ignore', windowsHide: true },
      )

      await Promise.race([
        encerrou,
        new Promise((resolve) => setTimeout(resolve, 2_000)),
      ])
    }
    return
  }

  try {
    process.kill(-processo.pid, 'SIGTERM')
  } catch {
    // O processo já encerrou.
  }

  await Promise.race([
    encerrou,
    new Promise((resolve) => setTimeout(resolve, 2_000)),
  ])

  if (processo.exitCode === null && processo.signalCode === null) {
    try {
      process.kill(-processo.pid, 'SIGKILL')
    } catch {
      // O processo pode ter terminado entre a verificação e o sinal.
    }
    await Promise.race([
      encerrou,
      new Promise((resolve) => setTimeout(resolve, 2_000)),
    ])
  }
}

export async function iniciarApiE2E() {
  await verificarPortaLivre(5057)
  const caminhoApi = path.resolve(
    diretorioFrontend,
    '../backend/FichaDigital.Api/bin/Debug/net10.0/FichaDigital.Api.dll',
  )
  const diretorioBanco = mkdtempSync(
    path.join(tmpdir(), 'ficha-digital-e2e-'),
  )
  const caminhoBanco = path.join(diretorioBanco, 'ficha-digital.db')

  const processo = iniciar(
    'dotnet',
    [caminhoApi, '--urls', 'http://127.0.0.1:5057'],
    {
      cwd: diretorioFrontend,
      env: {
        ...process.env,
        ASPNETCORE_ENVIRONMENT: 'E2E',
        DatabaseProvider: 'Sqlite',
        ConnectionStrings__DefaultConnection: `Data Source=${caminhoBanco}`,
        ProfissionalInicial__Habilitado: 'true',
        ProfissionalInicial__NomeCompleto: 'Conta do Estúdio E2E',
        ProfissionalInicial__Email: 'proprietaria.e2e@example.com',
        ProfissionalInicial__Senha: 'Teste-E2E-Segura-123!',
      },
    },
  )

  const limpar = async () => {
    await encerrar(processo)
    // O diretório é criado exclusivamente por mkdtemp neste teste.
    const raizTemporaria = path.resolve(tmpdir())
    const destino = path.resolve(diretorioBanco)
    if (path.dirname(destino) !== raizTemporaria || !path.basename(destino).startsWith('ficha-digital-e2e-')) {
      throw new Error('Diretório temporário inesperado; limpeza cancelada.')
    }
    rmSync(destino, { recursive: true, force: true })
  }
  try {
    await aguardarUrl('http://127.0.0.1:5057/health/ready', processo)
    return limpar
  } catch (error) {
    await limpar()
    throw error
  }
}

export async function iniciarFrontendE2E() {
  await verificarPortaLivre(5173)
  const processo = iniciar(
    process.execPath,
    [
      path.resolve(diretorioFrontend, 'node_modules/vite/bin/vite.js'),
      '--host',
      '127.0.0.1',
    ],
    { cwd: diretorioFrontend },
  )

  try {
    await aguardarUrl('http://127.0.0.1:5173', processo)
    return () => encerrar(processo)
  } catch (error) {
    await encerrar(processo)
    throw error
  }
}
