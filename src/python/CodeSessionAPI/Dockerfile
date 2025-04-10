FROM python:3.11-alpine3.21
ARG FOUNDATIONALLM_VERSION
ENV FOUNDATIONALLM_VERSION=${FOUNDATIONALLM_VERSION:-0.0.0}

RUN apk add --no-cache gcc git cmake && \
    apk add --no-cache --virtual .build-deps build-base python3-dev musl-dev linux-headers && \
    apk add --no-cache --virtual .msodbcdeps unixodbc unixodbc-dev && \
    apk add --no-cache --virtual .mssqltoolsdeps libgcc libstdc++ && \
    apk cache clean

WORKDIR /code

RUN python3 -m pip install --no-cache-dir --upgrade pip
RUN python3 -m pip install --no-cache-dir --upgrade setuptools==70.3.0

RUN git clone --no-checkout https://github.com/apache/arrow.git /arrow \
    && cd /arrow \
    && git checkout tags/apache-arrow-19.0.1 \
    && cd cpp \
    && mkdir build \
    && cd build \
    && cmake -DARROW_CSV=ON -DARROW_JSON=ON -DARROW_FILESYSTEM=ON .. \
    && make -j$(nproc) \
    && make install

COPY ./CodeSessionAPI/requirements.txt /code/requirements.txt
RUN python3 -m pip install --no-cache-dir --upgrade -r /code/requirements.txt

COPY ./CodeSessionAPI /code

EXPOSE 80

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "80"]
LABEL org.opencontainers.image.source=https://github.com/solliancenet/foundationallm
LABEL ai.foundationallm.version=${FOUNDATIONALLM_VERSION:-0.0.0}
