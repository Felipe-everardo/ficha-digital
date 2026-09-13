import { spawn, spawnSync, type ChildProcess } from 'node:child_process'
import { mkdtempSync, rmSync } from 'node:fs'
import { tmpdir } from 'node:os'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const diretorioFrontend = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  '..',
)
const processos: ChildProcess[] = []

async function aguardarUrl(url: string) {
  const limite = Date.now() + 120_000

  while (Date.now() < limite) {
    try {
      const response = await fetch(url)
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
  processos.push(processo)
  return processo
}

async function encerrar(processo: ChildProcess) {
  if (!processo.pid || processo.exitCode !== null) return

  const encerrou = new Promise<void>((resolve) => {
    processo.once('exit', () => resolve())
  })

  if (process.platform === 'win32') {
    processo.kill('SIGKILL')
    await Promise.race([
      encerrou,
      new Promise((resolve) => setTimeout(resolve, 2_000)),
    ])

    if (processo.exitCode === null) {
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
}

export default async function configurarAmbienteE2E() {
  const caminhoApi = path.resolve(
    diretorioFrontend,
    '../backend/FichaDigital.Api/bin/Debug/net10.0/FichaDigital.Api.dll',
  )
  const diretorioBanco = mkdtempSync(
    path.join(tmpdir(), 'ficha-digital-e2e-'),
  )
  const caminhoBanco = path.join(diretorioBanco, 'ficha-digital.db')

  iniciar(
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
  iniciar(
    process.execPath,
    [
      path.resolve(diretorioFrontend, 'node_modules/vite/bin/vite.js'),
      '--host',
      '127.0.0.1',
    ],
    { cwd: diretorioFrontend },
  )

  try {
    await Promise.all([
      aguardarUrl('http://127.0.0.1:5057/health/ready'),
      aguardarUrl('http://127.0.0.1:5173'),
    ])
  } catch (error) {
    await Promise.all(processos.reverse().map(encerrar))
    rmSync(diretorioBanco, { recursive: true, force: true })
    throw error
  }

  return async () => {
    await Promise.all(processos.reverse().map(encerrar))
    rmSync(diretorioBanco, { recursive: true, force: true })
  }
}
