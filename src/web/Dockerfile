# Multi-stage build from the Next.js docs example "with-docker": the first stage installs and builds with output "standalone",
# the second runs the small standalone folder on a plain Node image. Built from the repository root: docker build -f src/web/Dockerfile .
# .env.local is not copied (see .dockerignore); GATEWAY_URL comes from the compose file as an environment variable.
FROM node:22-alpine AS build
WORKDIR /source
COPY src/web/package.json src/web/package-lock.json ./
RUN npm ci
COPY src/web/ ./
ENV NEXT_TELEMETRY_DISABLED=1
RUN npm run build

FROM node:22-alpine
WORKDIR /app
ENV NODE_ENV=production
ENV HOSTNAME=0.0.0.0
ENV PORT=3000
COPY --from=build /source/public ./public
COPY --from=build /source/.next/standalone ./
COPY --from=build /source/.next/static ./.next/static
EXPOSE 3000
CMD ["node", "server.js"]
