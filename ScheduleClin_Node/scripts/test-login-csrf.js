const { createApp } = require('../src/app');

async function main() {
  const app = createApp();
  const server = app.listen(0, async () => {
    const port = server.address().port;
    const base = `http://127.0.0.1:${port}`;
    const jar = [];

    function storeCookies(res) {
      const raw = typeof res.headers.getSetCookie === 'function'
        ? res.headers.getSetCookie()
        : [res.headers.get('set-cookie')].filter(Boolean);
      for (const c of raw) {
        const name = c.split('=')[0];
        jar.filter((x) => !x.startsWith(`${name}=`));
        jar.push(c.split(';')[0]);
      }
    }

    const cookieHeader = () => jar.join('; ');

    const get = await fetch(`${base}/account/login`, { redirect: 'manual' });
    storeCookies(get);
    const html = await get.text();
    const token = html.match(/name="_csrf" value="([^"]+)"/)[1];

    const post = await fetch(`${base}/account/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        Cookie: cookieHeader(),
      },
      body: new URLSearchParams({
        email: 'gestor@scheduleclin.local',
        password: 'wrong',
        _csrf: token,
        returnUrl: '',
      }),
      redirect: 'manual',
    });

    const body = await post.text();
    console.log('GET /account/login:', get.status);
    console.log('POST /account/login:', post.status, body.includes('Credenciais') ? '(form ok)' : body.slice(0, 80));
    console.log('Cookies stored:', jar.map((c) => c.split('=')[0]).join(', '));

    server.close();
  });
}

main().catch(console.error);
