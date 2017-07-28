f32 testVertices[] ATTRIBUTE_ALIGN(32) = {
     1.00F, -1.00F, -1.00F,
     1.00F, -1.00F,  1.00F,
    -1.00F, -1.00F,  1.00F,
    -1.00F, -1.00F, -1.00F,
     1.00F,  1.00F, -1.00F,
     1.00F,  1.00F,  1.00F,
    -1.00F,  1.00F,  1.00F,
    -1.00F,  1.00F, -1.00F,
     0.00F, -1.00F,  0.00F,
     0.00F,  1.00F,  0.00F,
     1.00F,  0.00F,  0.00F,
     0.00F,  0.00F,  1.00F,
    -1.00F,  0.00F,  0.00F,
     0.00F,  0.00F, -1.00F
};

u16 testIndices[] ATTRIBUTE_ALIGN(32) = {
        1,     3,     0, 
        7,     5,     4, 
        4,     1,     0, 
        5,     2,     1, 
        2,     7,     3, 
        0,     7,     4, 
        1,     2,     3, 
        7,     6,     5, 
        4,     5,     1, 
        5,     6,     2, 
        2,     6,     7, 
        0,     3,     7
};

f32 testTexCoords[] ATTRIBUTE_ALIGN(32) = {
        0,     1,     2, 
        3,     4,     5, 
        6,     7,     2, 
        8,     9,    10, 
       11,    12,     1, 
       13,    14,     5, 
        0,    15,     1, 
        3,    16,     4, 
        6,    17,     7, 
        8,    16,     9, 
       11,    18,    12, 
       13,    19,    14
};

struct Model test = {
    .vertices        = testVertices,
    .verticesAmount  = 14,
    .indices         = testIndices,
    .indicesAmount   = 36,
    .texCoords       = testTexCoords,
    .texCoordsAmount = 36
};


