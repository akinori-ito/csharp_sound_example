library(tuneR)
#library(dplyr)

# 96kHz sampled signal
x <- readWave("test.wav")
carrier<-sine(30000,samp.rate=96000,duration=length(x),xunit="samples")
y <- x@left*carrier@left

# 200ms silence
sil <- rep(0,96000*0.2)
x0 <- c(sil,x@left)
y0 <- c(y,sil)
newsig <- normalize(Wave(x0+y0,samp.rate=96000,bit=16),unit="16")
writeWave(newsig,"embedded.wav")
