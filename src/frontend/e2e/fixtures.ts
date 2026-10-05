import { test as base, expect } from '@playwright/test'
import { iniciarApiE2E } from './servidores'

export { expect }

export const test = base.extend<{ apiIsolada: undefined }>({
  // eslint-disable-next-line no-empty-pattern -- Esta fixture não depende de outras fixtures.
  apiIsolada: [async ({}, executar) => {
    const encerrar = await iniciarApiE2E()
    try {
      await executar(undefined)
    } finally {
      await encerrar()
    }
  }, { auto: true, timeout: 120_000 }],
})
