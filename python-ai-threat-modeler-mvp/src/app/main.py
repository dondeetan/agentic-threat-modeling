from fastapi import FastAPI
from src.app.api.routes import router
from src.app.core.config import settings
from src.app.core.logging import configure_logging

configure_logging()

app = FastAPI(title=settings.app_name)
app.include_router(router)
